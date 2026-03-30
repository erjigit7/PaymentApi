namespace PaymentApi.Api.Middleware;

public sealed class RequestPipelineMiddleware
{
    private readonly RequestDelegate _next;

    public RequestPipelineMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context) => _next(context);
}
