namespace FitSync.Shared.Features.RateLimiting;

using Microsoft.Extensions.DependencyInjection;

public static class RateLimitingFeature
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        return services.AddSingleton<IRateLimiter, RateLimiter>();
    }
}