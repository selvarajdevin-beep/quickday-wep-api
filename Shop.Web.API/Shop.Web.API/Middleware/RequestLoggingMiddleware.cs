using System.Diagnostics;

namespace Shop.Web.API.Middleware
{
    /// <summary>
    /// Logs every incoming request with method, path, status code, and duration.
    /// Placed AFTER ExceptionHandlingMiddleware so status codes are final.
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            var sw = Stopwatch.StartNew();
            await _next(ctx);
            sw.Stop();

            _logger.LogInformation(
                "{Method} {Path} → {StatusCode} in {ElapsedMs}ms | IP: {Ip}",
                ctx.Request.Method,
                ctx.Request.Path,
                ctx.Response.StatusCode,
                sw.ElapsedMilliseconds,
                ctx.Connection.RemoteIpAddress);
        }
    }
}
