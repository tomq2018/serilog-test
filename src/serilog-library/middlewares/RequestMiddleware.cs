using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Serilog.AspNetCore;
using Serilog.Events;
using Serilog.Extensions.Hosting;
using Serilog.Parsing;
using System.Diagnostics;
using Serilog;
using System.Linq;

namespace serilog_library.middlewares;

public class RequestMiddleware
{
    readonly RequestDelegate _next;
    readonly DiagnosticContext _diagnosticContext;
    readonly MessageTemplate _messageTemplate;
    readonly Action<IDiagnosticContext, HttpContext>? _enrichDiagnosticContext;
    readonly Func<HttpContext, double, Exception?, LogEventLevel> _getLevel;
    readonly Func<HttpContext, string, double, int, IEnumerable<LogEventProperty>> _getMessageTemplateProperties;
    readonly ILogger? _logger;
    readonly bool _includeQueryInRequestPath;
    static readonly LogEventProperty[] NoProperties = new LogEventProperty[0];

    public RequestMiddleware(RequestDelegate next, DiagnosticContext diagnosticContext, RequestLoggingOptions options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _diagnosticContext = diagnosticContext ?? throw new ArgumentNullException(nameof(diagnosticContext));

        _getLevel = options.GetLevel;
        _enrichDiagnosticContext = options.EnrichDiagnosticContext;
        _messageTemplate = new MessageTemplateParser().Parse(options.MessageTemplate);
        _logger = options.Logger?.ForContext<RequestMiddleware>();
        _includeQueryInRequestPath = options.IncludeQueryInRequestPath;
        _getMessageTemplateProperties = options.GetMessageTemplateProperties;
    }

    // ReSharper disable once UnusedMember.Global
    public async Task Invoke(HttpContext httpContext)
    {
        if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

        var start = Stopwatch.GetTimestamp();

        var collector = _diagnosticContext.BeginCollection();

        // var genergeId = Guid.NewGuid().ToString();

        // _diagnosticContext.Set("GuidID", genergeId);
        try
        {
            await _next(httpContext);
            var elapsedMs = GetElapsedMilliseconds(start, Stopwatch.GetTimestamp());
            var statusCode = httpContext.Response.StatusCode;
            LogCompletion(httpContext, collector, statusCode, elapsedMs, null);
        }
        catch (Exception ex)
            // Never caught, because `LogCompletion()` returns false. This ensures e.g. the developer exception page is still
            // shown, although it does also mean we see a duplicate "unhandled exception" event from ASP.NET Core.
            when (LogCompletion(httpContext, collector, 500, GetElapsedMilliseconds(start, Stopwatch.GetTimestamp()), ex))
        {
        }
        finally
        {
            collector.Dispose();
        }
    }

    bool LogCompletion(HttpContext httpContext, DiagnosticContextCollector collector, int statusCode, double elapsedMs, Exception? ex)
    {
        var logger = _logger ?? Log.ForContext<RequestMiddleware>();
        var level = _getLevel(httpContext, elapsedMs, ex);

        if (!logger.IsEnabled(level)) return false;

        // Enrich diagnostic context
        _enrichDiagnosticContext?.Invoke(_diagnosticContext, httpContext);

        if (!collector.TryComplete(out var collectedProperties, out var collectedException))
            collectedProperties = NoProperties;

        // Last-in (correctly) wins...
        var properties = collectedProperties.Concat(_getMessageTemplateProperties(httpContext, GetPath(httpContext, _includeQueryInRequestPath), elapsedMs, statusCode));

        var propertiesDictionary = new Dictionary<string, string>
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };

        var kvpList = propertiesDictionary
            .Select(p => new KeyValuePair<ScalarValue, LogEventPropertyValue>(
                new ScalarValue(p.Key), new ScalarValue(p.Value)))
            .ToList();
        var testDictionary = new LogEventProperty("testDictionary", new DictionaryValue(kvpList));

        var a = new LogEventProperty("teststring", new ScalarValue("123"));
        // var b = new LogEventProperty("sessionID", new ScalarValue(new Guid().ToString()));

        // properties = properties.Append(b);
        properties = properties.Append(a);
        properties = properties.Append(testDictionary);

        var evt = new LogEvent(DateTimeOffset.Now, level, ex ?? collectedException, _messageTemplate, properties);
        logger.Write(evt);

        return false;
    }

    static double GetElapsedMilliseconds(long start, long stop)
    {
        return (stop - start) * 1000 / (double)Stopwatch.Frequency;
    }

    static string GetPath(HttpContext httpContext, bool includeQueryInRequestPath)
    {
        /*
            In some cases, like when running integration tests with WebApplicationFactory<T>
            the Path returns an empty string instead of null, in that case we can't use
            ?? as fallback.
        */
        var requestPath = includeQueryInRequestPath
            ? httpContext.Features.Get<IHttpRequestFeature>()?.RawTarget
            : httpContext.Features.Get<IHttpRequestFeature>()?.Path;
        if (string.IsNullOrEmpty(requestPath))
        {
            requestPath = httpContext.Request.Path.ToString();
        }

        return requestPath!;
    }
}