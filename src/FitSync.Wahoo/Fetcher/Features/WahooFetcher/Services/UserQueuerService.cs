namespace FitSync.Wahoo.Fetcher.Features.WahooFetcher.Services;

using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher;
using FitSync.Wahoo.Fetcher.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class UserQueuerService(
    FitSyncDbContext dbContext,
    ILogger<UserQueuerService> logger,
    IOptions<WahooFetcherOptions> options,
    IDestinationGate destinationGate
) : UserQueuerServiceBase(destinationGate, logger)
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly ILogger<UserQueuerService> logger = logger;
    private readonly IOptions<WahooFetcherOptions> options = options;
    private readonly TimeSpan lockDuration = TimeSpan.FromMinutes(20);

    protected override string SourceServiceType => ServiceTypes.Wahoo;

    protected override async Task<List<Guid>> GetCandidateUserIdsAsync()
    {
        DateTime now = DateTime.UtcNow;

        List<Guid> candidateUserIds = await this.dbContext.FetcherConfigs
            .Where(f => f.Integration.ServiceType == ServiceTypes.Wahoo)
            .Where(f => f.NextFetchTime == null || f.NextFetchTime <= now)
            .Where(f => f.WorkerLockId == null || f.LockExpiry <= now)
            .OrderBy(f => f.NextFetchTime ?? DateTime.MinValue)
            .Take(this.options.Value.MaxParallelUsers)
            .Select(f => f.Integration.UserId)
            .ToListAsync();

        this.logger.LogInformation("Found {Count} Wahoo candidate users before destination gate.", candidateUserIds.Count);
        return candidateUserIds;
    }

    protected override async Task<User[]> ClaimUsersAsync(List<Guid> eligibleUserIds)
    {
        DateTime now = DateTime.UtcNow;
        string instanceId = this.options.Value.InstanceId;
        DateTime newLockExpiry = now.Add(this.lockDuration);

        List<Guid> eligibleConfigIds = await this.dbContext.FetcherConfigs
            .Where(f => f.Integration.ServiceType == ServiceTypes.Wahoo
                     && eligibleUserIds.Contains(f.Integration.UserId))
            .Where(f => f.NextFetchTime == null || f.NextFetchTime <= now)
            .Where(f => f.WorkerLockId == null || f.LockExpiry <= now)
            .Select(f => f.Id)
            .ToListAsync();

        if (eligibleConfigIds.Count == 0)
            return [];

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

    public override async Task<bool> ReleaseUsersAsync(User[] users)
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
