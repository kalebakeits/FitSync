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
        IReadOnlyList<RateLimit> limits,
        CancellationToken cancellationToken = default
    )
    {
        IDatabase db = this.redis.GetDatabase();
        string service = serviceType.ToString().ToLower();
        DateTime now = DateTime.UtcNow;

        foreach (RateLimit limit in limits)
        {
            string key = GetWindowKey(service, now, limit.WindowMinutes);
            long count = await db.StringIncrementAsync(key);

            if (count == 1)
                await db.KeyExpireAsync(key, TimeSpan.FromMinutes(limit.WindowMinutes + 1));

            if (count > limit.Cap)
            {
                await db.StringDecrementAsync(key);
                this.logger.LogWarning(
                    "{Service} rate limited on {Window}m window (cap {Cap}). Suffering from success.",
                    serviceType,
                    limit.WindowMinutes,
                    limit.Cap
                );
                return true;
            }
        }

        this.logger.LogDebug(
            "{Service} rate limits not reached. Get more users king...",
            serviceType
        );
        return false;
    }

    private static string GetWindowKey(string service, DateTime now, int windowMinutes)
    {
        // Bucket the current time into the window size so all requests within
        // the same window share the same Redis key.
        long bucketIndex = (long)(now - DateTime.UnixEpoch).TotalMinutes / windowMinutes;
        return $"ratelimit:{service}:{windowMinutes}m:{bucketIndex}";
    }
}
