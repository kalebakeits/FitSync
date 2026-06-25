namespace FitSync.Api.Features.OAuth;

using FitSync.Api.Features.OAuth.Services;

public static class OAuthFeature
{
    public static IServiceCollection AddOAuthFeature(this IServiceCollection services)
    {
        services.AddScoped<IOAuthAuthorizationService, OAuthAuthorizationService>();
        return services;
    }
}
