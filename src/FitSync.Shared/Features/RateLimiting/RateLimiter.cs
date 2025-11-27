namespace FitSync.Shared.Features.RateLimiting;

using FitSync.Database.Enums;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

public class RateLimiter(IConnectionMultiplexer redis, ILogger<RateLimiter> logger) : IRateLimiter
{
    private readonly IConnectionMultiplexer redis = redis;
    private readonly ILogger<RateLimiter> logger = logger;

    public async Task<bool> RateLimitedReachedAsync(
        ServiceType serviceType,
        int requestCap = 50,
        CancellationToken cancellationToken = default
    )
    {
        IDatabase db = this.redis.GetDatabase();

        // Key format: "ratelimit:garmin:2024-11-27-14:30"
        string currentMinute = DateTime.UtcNow.ToString("yyyy-MM-dd-HH:mm");
        string key = $"ratelimit:{serviceType.ToString().ToLower()}:{currentMinute}";

        long newCount = await db.StringIncrementAsync(key);

        if (newCount == 1)
        {
            await db.KeyExpireAsync(key, TimeSpan.FromMinutes(2));
        }

        if (newCount > requestCap)
        {
            await db.StringDecrementAsync(key);
            this.logger.LogWarning(
                "{Service} is being rate limited. Suffering from success.",
                serviceType
            );
            return true;
        }
        this.logger.LogDebug(
            "{Service} rate limit not reached. Get more users king...",
            serviceType
        );
        return false;
    }
}
