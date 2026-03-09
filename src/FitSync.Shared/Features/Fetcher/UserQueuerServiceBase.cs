namespace FitSync.Shared.Features.Fetcher;

using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Configuration;
using FitSync.Shared.Features.Fetcher.Services;
using FitSync.Shared.Features.GlobalVariables.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class UserQueuerService(
    GlobalVariables globalVariables,
    IDestinationGate destinationGate,
    ILogger<UserQueuerService> logger,
    FitSyncDbContext dbContext,
    IOptions<FetcherOptions> options
) : IUserQueuerService
{
    private readonly GlobalVariables globalVariables = globalVariables;
    private readonly IDestinationGate destinationGate = destinationGate;
    private readonly ILogger<UserQueuerService> logger = logger;
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly IOptions<FetcherOptions> options = options;
    private readonly TimeSpan lockDuration = TimeSpan.FromMinutes(20);

    public async Task<User[]> GetDueUsersAsync()
    {
        DateTime now = DateTime.UtcNow;

        List<Guid> candidates = await this.dbContext.FetcherConfigs.Where(
            f => f.Integration.ServiceType == this.globalVariables.ServiceName
        )
            .Where(f => f.NextFetchTime == null || f.NextFetchTime <= now)
            .Where(f => f.WorkerLockId == null || f.LockExpiry <= now)
            .OrderBy(f => f.NextFetchTime ?? DateTime.MinValue)
            .Take(this.options.Value.MaxParallelUsers)
            .Select(f => f.Integration.UserId)
            .ToListAsync();

        if (candidates.Count == 0)
            return [];

        List<Guid> eligible = await this.destinationGate.FilterEligibleAsync(
            this.globalVariables.ServiceName,
            candidates
        );

        if (eligible.Count == 0)
        {
            this.logger.LogInformation(
                "No eligible {Source} users after destination gate check.",
                this.globalVariables.ServiceName
            );
            return [];
        }

        return await this.ClaimUsersAsync(eligible);
    }

    public async Task<bool> ReleaseUsersAsync(User[] users)
    {
        Guid[] userIds = users.Select(u => u.Id).ToArray();
        string instanceId = this.options.Value.InstanceId;

        List<FetcherConfig> claimed = await this.dbContext.FetcherConfigs.Include(
            f => f.Integration
        )
            .Where(
                f =>
                    f.Integration.ServiceType == this.globalVariables.ServiceName
                    && userIds.Contains(f.Integration.UserId)
                    && f.WorkerLockId == instanceId
            )
            .ToListAsync();

        if (claimed.Count == 0)
        {
            this.logger.LogWarning(
                "Worker {InstanceId} found no {Source} configs to release.",
                instanceId,
                this.globalVariables.ServiceName
            );
            return false;
        }

        DateTime nextFetchTime = DateTime.UtcNow.AddMinutes(this.options.Value.FetchIntervalMinutes);
        DateTime now = DateTime.UtcNow;

        foreach (FetcherConfig config in claimed)
        {
            config.WorkerLockId = null;
            config.LockExpiry = null;
            config.NextFetchTime = nextFetchTime;
            config.UpdatedAt = now;
        }

        int released = await this.dbContext.SaveChangesAsync();
        this.logger.LogInformation(
            "Released {Count} {Source} user configurations.",
            released,
            this.globalVariables.ServiceName
        );
        return released > 0;
    }

    private async Task<User[]> ClaimUsersAsync(List<Guid> eligibleUserIds)
    {
        DateTime now = DateTime.UtcNow;
        string instanceId = this.options.Value.InstanceId;
        DateTime newLockExpiry = now.Add(this.lockDuration);

        List<Guid> eligibleConfigIds = await this.dbContext.FetcherConfigs.Where(
            f =>
                f.Integration.ServiceType == this.globalVariables.ServiceName
                && eligibleUserIds.Contains(f.Integration.UserId)
                && (f.NextFetchTime == null || f.NextFetchTime <= now)
                && (f.WorkerLockId == null || f.LockExpiry <= now)
        )
            .Select(f => f.Id)
            .ToListAsync();

        if (eligibleConfigIds.Count == 0)
            return [];

        int updatedCount = await this.dbContext.FetcherConfigs.Where(
            f => eligibleConfigIds.Contains(f.Id) && (f.WorkerLockId == null || f.LockExpiry <= now)
        )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(f => f.WorkerLockId, instanceId)
                        .SetProperty(f => f.LockExpiry, newLockExpiry)
                        .SetProperty(f => f.UpdatedAt, now)
            );

        if (updatedCount == 0)
            return [];

        Guid[] userIds = await this.dbContext.FetcherConfigs.Include(f => f.Integration)
            .Where(
                f =>
                    eligibleConfigIds.Contains(f.Id)
                    && f.WorkerLockId == instanceId
                    && f.LockExpiry == newLockExpiry
            )
            .Select(f => f.Integration.UserId)
            .ToArrayAsync();

        this.logger.LogInformation(
            "Claimed {Count} {Source} users.",
            userIds.Length,
            this.globalVariables.ServiceName
        );
        return await this.dbContext.Users.Where(u => userIds.Contains(u.Id)).ToArrayAsync();
    }
}
