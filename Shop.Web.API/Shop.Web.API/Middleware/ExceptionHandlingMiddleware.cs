using Shop.Web.API.Exceptions;
using Shop.Web.API.Models.Responses;
using System.Text.Json;

namespace Shop.Web.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (AppException ex)
            {
                // Known domain error — log at Warning level (no stack trace needed)
                _logger.LogWarning(
                    "Domain exception [{Code}] on {Method} {Path}: {Message}",
                    ex.ErrorCode,
                    ctx.Request.Method,
                    ctx.Request.Path,
                    ex.Message);

                await WriteJsonResponse(
                    ctx,
                    ex.HttpStatus,
                    ApiResponse<object>.Fail(ex.Message, ex.ErrorCode));
            }
            catch (Exception ex)
            {
                // Unexpected error — log full details, return generic message to client
                _logger.LogError(
                    ex,
                    "Unhandled exception on {Method} {Path}",
                    ctx.Request.Method,
                    ctx.Request.Path);

                await WriteJsonResponse(
                    ctx,
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        "Something went wrong on our end. Please try again or contact support.",
                        "INTERNAL_ERROR"));
            }
        }

        private static async Task WriteJsonResponse<T>(
            HttpContext ctx,
            int statusCode,
            ApiResponse<T> body)
        {
            ctx.Response.StatusCode = statusCode;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(body, _jsonOpts));
        }
    }
}
