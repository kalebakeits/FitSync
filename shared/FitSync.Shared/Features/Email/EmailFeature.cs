namespace FitSync.Shared.Features.Email;

using FitSync.Shared.Features.Email.Services;
using Microsoft.Extensions.DependencyInjection;

public static class EmailFeature
{
    public static IServiceCollection AddEmailService(this IServiceCollection services)
    {
        services.AddScoped<IEmailService, EmailService>();
        return services;
    }
}
