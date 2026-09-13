namespace FitSync.Api.Features.Tokens;

using FitSync.Api.Features.Tokens.Services;
using Microsoft.Extensions.DependencyInjection;

public static class TokensFeature
{
    public static IServiceCollection AddTokensFeature(this IServiceCollection services)
    {
        services.AddScoped<IApiTokenService, ApiTokenService>();
        return services;
    }
}
