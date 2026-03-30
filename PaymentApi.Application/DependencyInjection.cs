using Microsoft.Extensions.DependencyInjection;
using PaymentApi.Application.Interfaces;
using PaymentApi.Application.Services;

namespace PaymentApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPaymentService, PaymentService>();

        return services;
    }
}
