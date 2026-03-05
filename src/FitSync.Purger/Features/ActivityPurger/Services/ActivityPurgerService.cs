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
        this.logger.LogInformation("Starting activity purge cycle");

        int totalDeleted = 0;

        foreach (KeyValuePair<string, int> entry in this.options.Value.SourceLookbackDays)
        {
            string sourceType = entry.Key;
            int lookbackDays = entry.Value;
            DateTime cutoff = DateTime.UtcNow.AddDays(-(lookbackDays + 1));

            this.logger.LogInformation(
                "Purging activities for source {Source} older than {Cutoff} (lookback {LookbackDays} days)",
                sourceType,
                cutoff,
                lookbackDays
            );

            int deleted = await this.dbContext.Activities
                .Where(a => a.Source == sourceType && a.ActivityDate < cutoff)
                .ExecuteDeleteAsync(ct);

            this.logger.LogInformation(
                "Purged {Count} activities for source {Source}",
                deleted,
                sourceType
            );

            totalDeleted += deleted;
        }

        this.logger.LogInformation(
            "Activity purge cycle complete. Total deleted: {TotalDeleted}",
            totalDeleted
        );
    }
}
