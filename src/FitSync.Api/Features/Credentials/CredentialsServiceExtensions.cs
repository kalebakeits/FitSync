using FitSync.Api.Features.Credentials.Services;

namespace FitSync.Api.Features.Credentials;

public static class CredentialsServiceExtensions
{
    public static IServiceCollection AddCredentialsFeature(this IServiceCollection services)
    {
        services.AddScoped<ICredentialsService, CredentialsService>();

        services.AddScoped<IServiceCredentialHandler, ZwiftCredentialHandler>();
        services.AddScoped<IServiceCredentialHandler, GarminCredentialHandler>();
        services.AddScoped<ServiceCredentialHandlerFactory>();

        return services;
    }
}
