namespace FitSync.ZwiftFetcher.Features.ZwiftFetcher.Services;

using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.Services;
using FitSync.ZwiftFetcher.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Handles selecting and locking Zwift users for processing based on scheduling rules.
/// </summary>
public class UserQueuerService(
    FitSyncDbContext dbContext,
    ILogger<UserQueuerService> logger,
    IOptions<ZwiftFetcherOptions> options
) : IUserQueuerService
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly ILogger<UserQueuerService> logger = logger;
    private readonly IOptions<ZwiftFetcherOptions> options = options;
    private readonly TimeSpan lockDuration = TimeSpan.FromMinutes(20); // Safety margin for a single fetch cycle

    public async Task<User[]> GetDueUsersAsync()
    {
        DateTime now = DateTime.UtcNow;
        string instanceId = this.options.Value.InstanceId;
        DateTime newLockExpiry = now.Add(this.lockDuration);

        // Get eligible user IDs (those with valid Garmin credentials)
        var eligibleUserIds = await this.dbContext.ZwiftFetcherConfigs.Join(
            this.dbContext.UserCredentials.Where(
                uc =>
                    uc.ServiceType == ServiceTypes.Garmin
                    && uc.FailureCount < this.options.Value.MaxSequentialCredentialFailures
            ),
            config => config.UserId,
            cred => cred.UserId,
            (config, cred) => config
        )
            .Where(c => c.NextFetchTime == null || c.NextFetchTime <= now)
            .Where(c => c.WorkerLockId == null || c.LockExpiry <= now)
            .OrderBy(c => c.NextFetchTime ?? DateTime.MinValue)
            .Take(this.options.Value.MaxParallelUsers)
            .Select(c => c.UserId)
            .ToListAsync();

        if (eligibleUserIds.Count == 0)
        {
            this.logger.LogInformation(
                "No eligible users with valid destination credentials found."
            );
            return [];
        }

        // Atomically update the eligible records
        int updatedCount = await this.dbContext.ZwiftFetcherConfigs.Where(
            c => eligibleUserIds.Contains(c.UserId)
        )
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(c => c.WorkerLockId, instanceId)
                        .SetProperty(c => c.LockExpiry, newLockExpiry)
                        .SetProperty(c => c.UpdatedAt, now)
            );

        if (updatedCount == 0)
        {
            this.logger.LogInformation("No due users to claim.");
            return [];
        }

        // Select what we just claimed
        Guid[] userIds = await this.dbContext.ZwiftFetcherConfigs.Where(
            c => c.WorkerLockId == instanceId && c.LockExpiry == newLockExpiry
        )
            .Select(c => c.UserId)
            .ToArrayAsync();

        this.logger.LogInformation(
            "Successfully claimed {Count} users with lock ID {InstanceId}.",
            userIds.Length,
            instanceId
        );

        return await this.dbContext.Users.Where(u => userIds.Contains(u.Id)).ToArrayAsync();
    }

    public async Task<bool> ReleaseUsersAsync(User[] users)
    {
        // 1. Get the configs that were processed by this worker instance
        Guid[] userIds = users.Select(u => u.Id).ToArray();
        string instanceId = this.options.Value.InstanceId;

        List<ZwiftFetcherConfig> claimedConfigs = await this.dbContext.ZwiftFetcherConfigs.Where(
            c => userIds.Contains(c.UserId)
        )
            // Only release users this instance claimed. This prevents accidental release.
            .Where(c => c.WorkerLockId == instanceId)
            .ToListAsync();

        if (claimedConfigs.Count == 0)
        {
            this.logger.LogWarning(
                "Worker {InstanceId} found no configurations to release.",
                instanceId
            );
            return false;
        }

        // 2. Schedule the next fetch time and release the lock
        DateTime nextFetchTime = DateTime.UtcNow.AddMinutes(this.options.Value.PollIntervalMinutes);
        DateTime now = DateTime.UtcNow;

        foreach (var config in claimedConfigs)
        {
            config.WorkerLockId = null; // Release lock
            config.LockExpiry = null; // Clear expiry
            config.NextFetchTime = nextFetchTime; // Set next scheduled time
            config.UpdatedAt = now;
        }

        // 3. Save changes
        int releasedCount = await this.dbContext.SaveChangesAsync();

        this.logger.LogInformation(
            "Successfully released {Count} user configurations.",
            releasedCount
        );

        return releasedCount > 0;
    }
}
