namespace FitSync.Wahoo.Shared.WahooClient;

using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.DTOs;
using FitSync.Wahoo.Shared.WahooClient.DTOs;
using FitSync.Wahoo.Shared.WahooClient.Services;
using Microsoft.Extensions.Logging;

public class WahooClient(
    IWahooApiService apiService,
    IWahooActivityProcessor activityProcessor,
    ILogger<WahooClient> logger
) : IWahooClient
{
    private readonly IWahooApiService apiService = apiService;
    private readonly IWahooActivityProcessor activityProcessor = activityProcessor;
    private readonly ILogger<WahooClient> logger = logger;

    public async Task<List<FetchedActivity>> GetActivitiesAsync(
        Integration integration,
        int lookbackDays,
        CancellationToken cancellationToken = default
    )
    {
        List<WahooWorkoutDto> workouts = await this.apiService.FetchWorkoutsAsync(
            integration, lookbackDays, cancellationToken
        );

        List<FetchedActivity> activities = await this.activityProcessor.ProcessActivitiesAsync(
            workouts, cancellationToken
        );

        this.logger.LogInformation(
            "WahooClient processed {Count} activities for user {UserId}.",
            activities.Count, integration.UserId
        );

        return activities;
    }

    public Task<byte[]> DownloadFitFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default
    )
    {
        return this.activityProcessor.DownloadFitFileAsync(fileUrl, cancellationToken);
    }
}
