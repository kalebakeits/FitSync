namespace FitSync.Shared.Features.Fetcher.Services;

using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Configuration;
using FitSync.Shared.Features.Fetcher.DTOs;
using FitSync.Shared.Features.GlobalVariables.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class FetcherService(
    FitSyncDbContext dbContext,
    ILogger<FetcherService> logger,
    IOptions<FetcherOptions> options,
    IFetcherClient fetcherClient,
    GlobalVariables globalVariables
) : IFetcherService
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly ILogger<FetcherService> logger = logger;
    private readonly IOptions<FetcherOptions> options = options;
    private readonly IFetcherClient fetcherClient = fetcherClient;
    private readonly string serviceName = globalVariables.ServiceName;

    public async Task<List<FetchedActivity>> GetActivitiesAsync(
        User user,
        CancellationToken cancellationToken
    )
    {
        this.logger.LogInformation(
            "Starting {ServiceName} fetch for user {UserId}.",
            this.serviceName,
            user.Id
        );

        Integration? integration = await this.dbContext.Integrations.FirstOrDefaultAsync(
            i => i.UserId == user.Id && i.ServiceType == this.serviceName,
            cancellationToken
        );

        if (integration == null)
        {
            this.logger.LogError(
                "No {ServiceName} integration found for user {UserId}. Skipping.",
                this.serviceName,
                user.Id
            );
            return [];
        }

        List<FetchedActivity> activities;
        try
        {
            activities = await this.fetcherClient.GetActivitiesAsync(
                integration,
                this.options.Value.LookbackDays,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Failed to fetch {ServiceName} activities for user {UserId}.",
                this.serviceName,
                user.Id
            );
            return [];
        }

        if (activities.Count == 0)
            return [];

        List<string> externalIds = [.. activities.Select(a => a.ExternalActivityId)];
        HashSet<string> alreadyProcessed =
        [
            ..await this.dbContext.ProcessedActivities
                .Where(p => p.UserId == user.Id && p.Source == this.serviceName && externalIds.Contains(p.ExternalActivityId))
                .Select(p => p.ExternalActivityId)
                .ToListAsync(cancellationToken)
        ];

        List<FetchedActivity> newActivities = activities
            .Where(a => !alreadyProcessed.Contains(a.ExternalActivityId))
            .ToList();

        this.logger.LogInformation(
            "Found {New} new activities out of {Total} for user {UserId}.",
            newActivities.Count,
            activities.Count,
            user.Id
        );

        return newActivities;
    }
}
