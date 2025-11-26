using FitSync.Api.Features.Auth.Services;

namespace FitSync.Api.Features.Auth;

public static class AuthServiceExtensions
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
