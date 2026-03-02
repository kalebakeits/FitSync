namespace FitSync.Wahoo.Fetcher.Features.WahooFetcher.Services;

using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.Services;
using FitSync.Wahoo.Fetcher.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class UserQueuerService(
    FitSyncDbContext dbContext,
    ILogger<UserQueuerService> logger,
    IOptions<WahooFetcherOptions> options
) : IUserQueuerService
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly ILogger<UserQueuerService> logger = logger;
    private readonly IOptions<WahooFetcherOptions> options = options;
    private readonly TimeSpan lockDuration = TimeSpan.FromMinutes(20);

    public async Task<User[]> GetDueUsersAsync()
    {
        DateTime now = DateTime.UtcNow;
        string instanceId = this.options.Value.InstanceId;
        DateTime newLockExpiry = now.Add(this.lockDuration);

        int maxFailures = this.options.Value.MaxSequentialCredentialFailures;

        List<Guid> eligibleConfigIds = await this.dbContext.FetcherConfigs
            .Where(f => f.Integration.ServiceType == ServiceTypes.Wahoo)
            .Where(f => this.dbContext.Integrations.Any(
                g => g.UserId == f.Integration.UserId
                     && g.ServiceType == ServiceTypes.Garmin
                     && g.FailureCount < maxFailures
            ))
            .Where(f => f.NextFetchTime == null || f.NextFetchTime <= now)
            .Where(f => f.WorkerLockId == null || f.LockExpiry <= now)
            .OrderBy(f => f.NextFetchTime ?? DateTime.MinValue)
            .Take(this.options.Value.MaxParallelUsers)
            .Select(f => f.Id)
            .ToListAsync();

        if (eligibleConfigIds.Count == 0)
        {
            this.logger.LogInformation("No eligible Wahoo users with valid Garmin destination.");
            return [];
        }

        int updatedCount = await this.dbContext.FetcherConfigs
            .Where(f => eligibleConfigIds.Contains(f.Id))
            .ExecuteUpdateAsync(s =>
                s.SetProperty(f => f.WorkerLockId, instanceId)
                 .SetProperty(f => f.LockExpiry, newLockExpiry)
                 .SetProperty(f => f.UpdatedAt, now)
            );

        if (updatedCount == 0)
            return [];

        Guid[] userIds = await this.dbContext.FetcherConfigs
            .Include(f => f.Integration)
            .Where(f => eligibleConfigIds.Contains(f.Id) && f.WorkerLockId == instanceId && f.LockExpiry == newLockExpiry)
            .Select(f => f.Integration.UserId)
            .ToArrayAsync();

        this.logger.LogInformation("Claimed {Count} Wahoo users.", userIds.Length);
        return await this.dbContext.Users.Where(u => userIds.Contains(u.Id)).ToArrayAsync();
    }

    public async Task<bool> ReleaseUsersAsync(User[] users)
    {
        Guid[] userIds = users.Select(u => u.Id).ToArray();
        string instanceId = this.options.Value.InstanceId;

        List<FetcherConfig> claimed = await this.dbContext.FetcherConfigs
            .Include(f => f.Integration)
            .Where(f => f.Integration.ServiceType == ServiceTypes.Wahoo
                     && userIds.Contains(f.Integration.UserId)
                     && f.WorkerLockId == instanceId)
            .ToListAsync();

        if (claimed.Count == 0)
        {
            this.logger.LogWarning("Worker {InstanceId} found no Wahoo configs to release.", instanceId);
            return false;
        }

        DateTime nextFetchTime = DateTime.UtcNow.AddMinutes(this.options.Value.PollIntervalMinutes);
        DateTime now = DateTime.UtcNow;

        foreach (FetcherConfig config in claimed)
        {
            config.WorkerLockId = null;
            config.LockExpiry = null;
            config.NextFetchTime = nextFetchTime;
            config.UpdatedAt = now;
        }

        int released = await this.dbContext.SaveChangesAsync();
        this.logger.LogInformation("Released {Count} Wahoo configs.", released);
        return released > 0;
    }
}
