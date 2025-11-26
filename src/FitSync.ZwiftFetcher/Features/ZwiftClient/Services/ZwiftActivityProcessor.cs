namespace FitSync.ZwiftFetcher.Features.ZwiftClient.Services;

using FitSync.Shared.Features.Fetcher.DTOs;
using FitSync.ZwiftFetcher.Features.ZwiftClient.DTOs;
using Microsoft.Extensions.Logging;

public class ZwiftActivityProcessor(ILogger<ZwiftActivityProcessor> logger)
    : IZwiftActivityProcessor
{
    private readonly ILogger<ZwiftActivityProcessor> logger = logger;

    public async Task<List<FetchedActivity>> ProcessActivitiesAsync(
        ZwiftActivityDto[] activities,
        int lookbackDays,
        CancellationToken cancellationToken = default
    )
    {
        DateTime cutoff = DateTime.UtcNow.AddDays(-lookbackDays);
        List<FetchedActivity> fetchedActivities = [];

        this.logger.LogDebug(
            "Trimming user activities with maximum look back of {LookbackDays}",
            lookbackDays
        );

        foreach (var activity in activities)
        {
            DateTime activityStartDate = activity.GetStartDateTime();

            if (activityStartDate < cutoff)
            {
                this.logger.LogDebug("Skipping activity {Id} due to date cutoff", activity.Id);
                continue;
            }

            byte[] fitFileData = await this.DownloadFitFileAsync(activity, cancellationToken);

            fetchedActivities.Add(
                new FetchedActivity(
                    ExternalActivityId: activity.Id.ToString(),
                    Source: "Zwift",
                    ActivityDate: activityStartDate,
                    FileName: $"zwift_{activityStartDate:yyyyMMdd_HHmmss}_{activity.Id}.fit",
                    FitFileData: fitFileData,
                    Metadata: null,
                    ActivityName: activity.Name
                )
            );
        }

        return fetchedActivities;
    }

    private async Task<byte[]> DownloadFitFileAsync(
        ZwiftActivityDto activity,
        CancellationToken cancellationToken
    )
    {
        if (
            string.IsNullOrEmpty(activity.FitFileBucket)
            || string.IsNullOrEmpty(activity.FitFileKey)
        )
        {
            throw new InvalidOperationException(
                $"Activity {activity.Id} is missing FIT file details."
            );
        }

        string url = $"https://{activity.FitFileBucket}.s3.amazonaws.com/{activity.FitFileKey}";

        this.logger.LogDebug("Downloading FIT file from {Url}", url);

        using var client = new HttpClient();
        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        byte[] fitFileData = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        this.logger.LogInformation(
            "Successfully downloaded {Length} bytes for activity {Id}",
            fitFileData.Length,
            activity.Id
        );

        return fitFileData;
    }
}
