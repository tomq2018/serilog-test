using System.Diagnostics;
using Serilog;
using Serilog.Extensions.Hosting;

namespace serilog_library.Handlers;

public class RefitHttpHandler : DelegatingHandler
{
    private readonly DiagnosticContext _diagnosticContext;
    private readonly ILogger _logger;

    public RefitHttpHandler(DiagnosticContext diagnosticContext,
        ILogger logger)
    {
        _diagnosticContext = diagnosticContext ?? throw new ArgumentNullException(nameof(diagnosticContext));
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        // Call the inner handler.
        var response = await base.SendAsync(request, cancellationToken);

        // Log the response status and response time.
        _logger.Information("{Method} {Uri} responded {StatusCode} in {ElapsedMilliseconds} ms", request.Method, request.RequestUri, response.StatusCode, stopwatch.ElapsedMilliseconds);

        return response;
    }
}