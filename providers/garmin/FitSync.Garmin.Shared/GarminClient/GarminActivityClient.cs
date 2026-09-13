namespace FitSync.Garmin.Shared.GarminClient;

using FitSync.Database.Models;
using FitSync.Garmin.Shared.GarminClient.DTOs;
using FitSync.Garmin.Shared.GarminClient.Services;
using FitSync.Shared.Features.Fetcher.DTOs;
using Microsoft.Extensions.Logging;

public class GarminActivityClient(
    IGarminAuthService authService,
    IGarminApiService apiService,
    IGarminActivityProcessor activityProcessor,
    ILogger<GarminActivityClient> logger
) : IGarminActivityClient
{
    private readonly IGarminAuthService authService = authService;
    private readonly IGarminApiService apiService = apiService;
    private readonly IGarminActivityProcessor activityProcessor = activityProcessor;
    private readonly ILogger<GarminActivityClient> logger = logger;

    public async Task<List<FetchedActivity>> GetActivitiesAsync(
        Integration integration,
        int lookbackDays,
        CancellationToken cancellationToken = default
    )
    {
        await this.authService.EnsureAuthenticatedAsync(integration, cancellationToken);

        GarminActivityDto[] activities = await this.apiService.FetchActivitiesAsync(
            integration,
            lookbackDays,
            cancellationToken
        );

        List<FetchedActivity> fetched = await this.activityProcessor.ProcessActivitiesAsync(
            integration,
            activities,
            lookbackDays,
            cancellationToken
        );

        this.logger.LogInformation(
            "Fetched {Count} Garmin activities for user {UserId}.",
            fetched.Count,
            integration.UserId
        );

        return fetched;
    }
}
