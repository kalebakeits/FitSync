namespace FitSync.Api.Features.Auth;

using FitSync.Api.Features.Auth.Services;

public static class AuthServiceExtensions
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
