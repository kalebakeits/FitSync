namespace FitSync.Purger.Features.ActivityPurger.Services;

using FitSync.Database;
using FitSync.Purger.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class ActivityPurgerService(
    FitSyncDbContext dbContext,
    IOptions<PurgerOptions> options,
    ILogger<ActivityPurgerService> logger
) : IActivityPurgerService
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly IOptions<PurgerOptions> options = options;
    private readonly ILogger<ActivityPurgerService> logger = logger;

    public async Task PurgeAsync(CancellationToken ct)
    {
        this.logger.LogWarning("This is not a test. The Purge commences now.");

        int totalExecuted = 0;

        totalExecuted += await this.PurgeSoftDeletedAsync(ct);

        if (this.options.Value.EnableDataRetentionPurge)
            totalExecuted += await this.PurgeByDataRetentionAsync(ct);
        else
            this.logger.LogInformation(
                "Data retention purge is disabled. The living are safe — tonight."
            );

        if (totalExecuted == 0)
            this.logger.LogInformation(
                "The Purge is complete. No targets found. God bless FitSync."
            );
        else
            this.logger.LogWarning(
                "The Purge is complete. {Total} executed. God bless FitSync.",
                totalExecuted
            );
    }

    private async Task<int> PurgeSoftDeletedAsync(CancellationToken ct)
    {
        this.logger.LogInformation("Compliance purge commencing. The marked have been waiting.");

        int total = 0;

        foreach (KeyValuePair<string, int> entry in this.options.Value.SourceLookbackDays)
        {
            string sourceType = entry.Key;
            int lookbackDays = entry.Value;
            DateTime cutoff = DateTime.UtcNow.AddDays(-(lookbackDays + 1));

            this.logger.LogInformation(
                "{Source} activities marked before {Cutoff}. They had {LookbackDays} days.",
                sourceType,
                cutoff,
                lookbackDays
            );

            int deleted = await this.dbContext.Activities.Where(
                a => a.Source == sourceType && a.IsDeleted && a.DeletedAt < cutoff
            )
                .ExecuteDeleteAsync(ct);

            if (deleted > 0)
                this.logger.LogWarning(
                    "{Count} {Source} executed. May God have mercy on their rows.",
                    deleted,
                    sourceType
                );
            else
                this.logger.LogInformation(
                    "No {Source} targets found. Their time has not yet come.",
                    sourceType
                );

            total += deleted;
        }

        return total;
    }

    private async Task<int> PurgeByDataRetentionAsync(CancellationToken ct)
    {
        int retentionDays = this.options.Value.DataRetentionDays;
        DateTime cutoff = DateTime.UtcNow.AddDays(-retentionDays);

        this.logger.LogWarning(
            "This is not a test. The Data Retention Purge commences. All activities before {Cutoff} are targets.",
            cutoff
        );

        int deleted = await this.dbContext.Activities.Where(a => a.ActivityDate < cutoff)
            .ExecuteDeleteAsync(ct);

        if (deleted > 0)
            this.logger.LogWarning(
                "{Count} executed. The database is lighter. God bless FitSync.",
                deleted
            );
        else
            this.logger.LogInformation("No targets found. The database is already pure.");

        return deleted;
    }
}
