using PaymentApi.Api.Middleware;

namespace PaymentApi.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UsePaymentApiMiddleware(this WebApplication app)
    {
        app.UseMiddleware<RequestPipelineMiddleware>();
        return app;
    }
}
