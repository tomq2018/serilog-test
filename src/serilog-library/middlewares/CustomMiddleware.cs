using Microsoft.AspNetCore.Http;
using Serilog.Context;
using Serilog.Extensions.Hosting;
using System.Threading.Tasks;

namespace serilog_library.middlewares;

public class CustomMiddleware
{
    private readonly RequestDelegate _next;
    private readonly DiagnosticContext _diagnosticContext;

    public CustomMiddleware(RequestDelegate next,
        DiagnosticContext diagnosticContext)
    {
        _next = next;
        _diagnosticContext = diagnosticContext;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var genergeId = Guid.NewGuid().ToString();

        _diagnosticContext.Set("GuidID", genergeId);
        // 执行处理请求之前的操作
        // using (LogContext.PushProperty("GuidID", genergeId))
        // {
        //     await _next(context);
        // }
        await _next(context);

        // 执行处理请求之后的操作
    }
}
