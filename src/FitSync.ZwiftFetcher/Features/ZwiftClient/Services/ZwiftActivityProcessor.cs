namespace FitSync.ZwiftFetcher.Features.ZwiftClient.Services;

using FitSync.Database.Enums;
using FitSync.Shared.Features.Fetcher.DTOs;
using FitSync.Shared.Features.RateLimiting;
using FitSync.ZwiftFetcher.Configuration;
using FitSync.ZwiftFetcher.Features.ZwiftClient.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class ZwiftActivityProcessor(
    ILogger<ZwiftActivityProcessor> logger,
    IOptions<ZwiftFetcherOptions> options,
    IRateLimiter rateLimiter
) : IZwiftActivityProcessor
{
    private readonly ILogger<ZwiftActivityProcessor> logger = logger;
    private readonly IOptions<ZwiftFetcherOptions> options = options;
    private readonly IRateLimiter rateLimiter = rateLimiter;

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

        await Parallel.ForEachAsync(
            activities,
            async (activity, cancellationToken) =>
            {
                DateTime activityStartDate = activity.GetStartDateTime();
                bool withinCutoff = activityStartDate > cutoff;
                if (!withinCutoff)
                {
                    this.logger.LogDebug(
                        "Skipping activity {Id} due to date cutoff {Cutoff} vs activity start date {ActivityStartDate}",
                        activity.Id,
                        cutoff,
                        activityStartDate
                    );
                    return;
                }

                if (!activity.IsCompleted())
                {
                    this.logger.LogInformation(
                        "Skipping activity {Id} ({Name}) - activity is still in progress (no endDate)",
                        activity.Id,
                        activity.Name
                    );
                    return;
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
        );

        return fetchedActivities;
    }

    private async Task<byte[]> DownloadFitFileAsync(
        ZwiftActivityDto activity,
        CancellationToken cancellationToken
    )
    {
        ServiceType type = ServiceType.AmazonS3;
        int limit = this.options.Value.AmazonS3RateLimit;
        if (await this.rateLimiter.RateLimitedReachedAsync(type, limit, cancellationToken))
            return [];

        bool missingFileDetails =
            string.IsNullOrEmpty(activity.FitFileBucket)
            || string.IsNullOrEmpty(activity.FitFileKey);

        if (missingFileDetails)
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
