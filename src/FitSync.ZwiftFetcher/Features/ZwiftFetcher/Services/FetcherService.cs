namespace FitSync.ZwiftFetcher.Features.ZwiftFetcher.Services;

using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.DTOs;
using FitSync.Shared.Features.Fetcher.Services;
using FitSync.ZwiftFetcher.Configuration;
using FitSync.ZwiftFetcher.Features.ZwiftClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Implements the core logic for fetching activities from Zwift for a given user.
/// </summary>
public class FetcherService(
    FitSyncDbContext dbContext,
    IZwiftClient zwiftClient,
    ILogger<FetcherService> logger,
    IOptions<ZwiftFetcherOptions> options
) : IFetcherService
{
    private const string SourceName = "Zwift";
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly IZwiftClient zwiftClient = zwiftClient;
    private readonly ILogger<FetcherService> logger = logger;
    private readonly IOptions<ZwiftFetcherOptions> options = options;

    public async Task<List<FetchedActivity>> GetActivitiesAsync(
        User user,
        CancellationToken cancellationToken
    )
    {
        this.logger.LogInformation(
            "Starting fetch cycle for user {UserId} ({Username})",
            user.Id,
            user.Username
        );

        // 1. Retrieve the user's Zwift configuration
        ZwiftFetcherConfig? config = await this.dbContext.ZwiftFetcherConfigs.SingleOrDefaultAsync(
            c => c.UserId == user.Id,
            cancellationToken
        );

        if (config == null)
        {
            this.logger.LogError(
                "No ZwiftFetcherConfig found for user {UserId}. Skipping.",
                user.Id
            );
            return [];
        }

        // 2. Call the Zwift Client to get raw activities.
        // This handles token refresh, API call, and file download internally.
        List<FetchedActivity> zwiftActivities;
        try
        {
            zwiftActivities = await this.zwiftClient.GetActivitiesAsync(
                config,
                this.options.Value.LookbackDays,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Failed to retrieve activities from Zwift for user {UserId}",
                user.Id
            );
            // NOTE: You would typically update the config with an error state here before returning.
            return [];
        }

        if (zwiftActivities.Count == 0)
        {
            this.logger.LogInformation(
                "No activities returned from Zwift API for user {UserId}",
                user.Id
            );
            return [];
        }

        // 3. De-duplication: Filter out activities already processed
        // This replaces the Python's StateManager.is_processed file check with a database check.
        List<string> externalIds = zwiftActivities.Select(a => a.ExternalActivityId).ToList();

        List<string> processedExternalIds = await this.dbContext.ProcessedActivities.Where(
            p => p.UserId == user.Id && p.Source == SourceName
        )
            .Where(p => externalIds.Contains(p.ExternalActivityId))
            .Select(p => p.ExternalActivityId)
            .ToListAsync(cancellationToken);

        HashSet<string> alreadyProcessedSet = [.. processedExternalIds];

        List<FetchedActivity> newActivities = zwiftActivities
            .Where(a => !alreadyProcessedSet.Contains(a.ExternalActivityId))
            .ToList();

        this.logger.LogInformation(
            "Found {NewCount} new activities out of {TotalCount} total for user {UserId}",
            newActivities.Count,
            zwiftActivities.Count,
            user.Id
        );

        return newActivities;
    }
}
