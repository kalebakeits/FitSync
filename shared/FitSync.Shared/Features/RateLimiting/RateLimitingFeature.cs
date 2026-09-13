namespace FitSync.Shared.Features.RateLimiting;

using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

public static class RateLimitingFeature
{
    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        string redisConnectionString
    )
    {
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            ConfigurationOptions configuration = ConfigurationOptions.Parse(redisConnectionString);
            configuration.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(configuration);
        });

        return services.AddSingleton<IRateLimiter, RateLimiter>();
    }
}
