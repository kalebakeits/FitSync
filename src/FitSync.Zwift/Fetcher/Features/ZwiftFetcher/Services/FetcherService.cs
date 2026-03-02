namespace FitSync.Zwift.Fetcher.Features.ZwiftFetcher.Services;

using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.DTOs;
using FitSync.Shared.Features.Fetcher.Services;
using FitSync.Zwift.Fetcher.Configuration;
using FitSync.Zwift.Fetcher.Features.ZwiftClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        this.logger.LogInformation("Starting Zwift fetch for user {UserId}.", user.Id);

        Integration? integration = await this.dbContext.Integrations.FirstOrDefaultAsync(
            i => i.UserId == user.Id && i.ServiceType == ServiceTypes.Zwift,
            cancellationToken
        );

        if (integration == null)
        {
            this.logger.LogError("No Zwift integration found for user {UserId}. Skipping.", user.Id);
            return [];
        }

        List<FetchedActivity> activities;
        try
        {
            activities = await this.zwiftClient.GetActivitiesAsync(
                integration, this.options.Value.LookbackDays, cancellationToken
            );
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to fetch Zwift activities for user {UserId}.", user.Id);
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
