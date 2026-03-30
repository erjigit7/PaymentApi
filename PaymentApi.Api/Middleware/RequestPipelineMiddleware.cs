using System.Text.Json;
using PaymentApi.Api.Common;

namespace PaymentApi.Api.Middleware;

public sealed class RequestPipelineMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestPipelineMiddleware> _logger;

    public RequestPipelineMiddleware(
        RequestDelegate next,
        ILogger<RequestPipelineMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request error");

            await WriteErrorAsync(
                context,
                StatusCodes.Status400BadRequest,
                new ApiError("bad_request", ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized error");

            await WriteErrorAsync(
                context,
                StatusCodes.Status401Unauthorized,
                new ApiError("unauthorized", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled server error");

            await WriteErrorAsync(
                context,
                StatusCodes.Status500InternalServerError,
                new ApiError("internal_server_error", "An unexpected error occurred."));
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, ApiError error)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(error);
        await context.Response.WriteAsync(json);
    }
}