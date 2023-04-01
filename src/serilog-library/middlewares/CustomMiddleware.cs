using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace serilog_library.middlewares;

public class CustomMiddleware
{
    private readonly RequestDelegate _next;

    public CustomMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 执行处理请求之前的操作

        await _next(context);

        // 执行处理请求之后的操作
    }
}
