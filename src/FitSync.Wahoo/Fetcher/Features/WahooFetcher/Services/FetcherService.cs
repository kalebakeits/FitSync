namespace FitSync.Wahoo.Fetcher.Features.WahooFetcher.Services;

using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.DTOs;
using FitSync.Shared.Features.Fetcher.Services;
using FitSync.Wahoo.Fetcher.Configuration;
using FitSync.Wahoo.Shared.WahooClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class FetcherService(
    FitSyncDbContext dbContext,
    IWahooClient wahooClient,
    ILogger<FetcherService> logger,
    IOptions<WahooFetcherOptions> options
) : IFetcherService
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly IWahooClient wahooClient = wahooClient;
    private readonly ILogger<FetcherService> logger = logger;
    private readonly IOptions<WahooFetcherOptions> options = options;
    private const string SourceName = "Wahoo";

    public async Task<List<FetchedActivity>> GetActivitiesAsync(
        User user,
        CancellationToken cancellationToken
    )
    {
        this.logger.LogInformation("Starting Wahoo fetch for user {UserId}.", user.Id);

        Integration? integration = await this.dbContext.Integrations.FirstOrDefaultAsync(
            i => i.UserId == user.Id && i.ServiceType == ServiceTypes.Wahoo,
            cancellationToken
        );

        if (integration == null)
        {
            this.logger.LogError("No Wahoo integration found for user {UserId}. Skipping.", user.Id);
            return [];
        }

        List<FetchedActivity> activities;
        try
        {
            activities = await this.wahooClient.GetActivitiesAsync(
                integration, this.options.Value.LookbackDays, cancellationToken
            );
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to fetch Wahoo activities for user {UserId}.", user.Id);
            return [];
        }

        if (activities.Count == 0)
            return [];

        List<string> externalIds = activities.Select(a => a.ExternalActivityId).ToList();
        HashSet<string> alreadyProcessed = [
            ..await this.dbContext.ProcessedActivities
                .Where(p => p.UserId == user.Id && p.Source == SourceName && externalIds.Contains(p.ExternalActivityId))
                .Select(p => p.ExternalActivityId)
                .ToListAsync(cancellationToken)
        ];

        List<FetchedActivity> newActivities = activities
            .Where(a => !alreadyProcessed.Contains(a.ExternalActivityId))
            .ToList();

        this.logger.LogInformation(
            "Found {New} new activities out of {Total} for user {UserId}.",
            newActivities.Count, activities.Count, user.Id
        );

        return newActivities;
    }
}
