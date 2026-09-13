namespace FitSync.Wahoo.Shared.WahooClient.Services;

using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.DTOs;
using FitSync.Wahoo.Shared.WahooClient.DTOs;
using Microsoft.Extensions.Logging;

public class WahooActivityProcessor(HttpClient httpClient, ILogger<WahooActivityProcessor> logger)
    : IWahooActivityProcessor
{
    private readonly HttpClient httpClient = httpClient;
    private readonly ILogger<WahooActivityProcessor> logger = logger;

    public async Task<List<FetchedActivity>> ProcessActivitiesAsync(
        List<WahooWorkoutDto> workouts,
        CancellationToken cancellationToken = default
    )
    {
        List<FetchedActivity> results = [];

        foreach (WahooWorkoutDto workout in workouts)
        {
            string? fileUrl = workout.WorkoutSummary?.File?.Url;

            if (string.IsNullOrEmpty(fileUrl))
            {
                this.logger.LogWarning(
                    "Skipping workout {WorkoutId} - no FIT file URL.",
                    workout.Id
                );
                continue;
            }

            byte[] fitData = await this.DownloadFitFileAsync(fileUrl, cancellationToken);

            results.Add(
                new FetchedActivity(
                    ExternalActivityId: workout.Id.ToString(),
                    Source: ServiceTypes.Wahoo,
                    ActivityDate: workout.Starts,
                    FileName: $"wahoo_{workout.Id}.fit",
                    FitFileData: fitData,
                    ActivityName: workout.Name
                )
            );
        }

        return results;
    }

    public async Task<byte[]> DownloadFitFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogDebug("Downloading FIT file from {Url}.", fileUrl);
        HttpResponseMessage response = await this.httpClient.GetAsync(fileUrl, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
