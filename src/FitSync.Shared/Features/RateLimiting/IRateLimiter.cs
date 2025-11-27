namespace FitSync.Shared.Features.RateLimiting;

using FitSync.Database.Enums;

public interface IRateLimiter
{
    Task<bool> RateLimitedReachedAsync(
        ServiceType serviceType,
        int RequestCap,
        CancellationToken cancellationToken = default
    );
}
