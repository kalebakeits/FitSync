namespace FitSync.Garmin.Shared.GarminClient.Services;

using System.Collections.Concurrent;
using FitSync.Database.Models;
using FitSync.Garmin.Shared.GarminClient.DTOs;
using FitSync.Shared.Features.Fetcher.DTOs;
using Microsoft.Extensions.Logging;

public class GarminActivityProcessor(
    IGarminFitDownloader fitDownloader,
    ILogger<GarminActivityProcessor> logger
) : IGarminActivityProcessor
{
    private readonly IGarminFitDownloader fitDownloader = fitDownloader;
    private readonly ILogger<GarminActivityProcessor> logger = logger;

    public async Task<List<FetchedActivity>> ProcessActivitiesAsync(
        Integration integration,
        GarminActivityDto[] activities,
        int lookbackDays,
        CancellationToken cancellationToken = default
    )
    {
        DateTime cutoff = DateTime.UtcNow.AddDays(-lookbackDays);
        ConcurrentBag<FetchedActivity> fetchedActivities = [];

        this.logger.LogDebug(
            "Trimming Garmin activities for user {UserId} with maximum look back of {LookbackDays}.",
            integration.UserId,
            lookbackDays
        );

        await Parallel.ForEachAsync(
            activities,
            async (activity, token) =>
            {
                try
                {
                    DateTime activityStartDate = activity.GetStartDateTime();

                    if (activityStartDate <= cutoff)
                    {
                        this.logger.LogInformation(
                            "Skipping Garmin activity {ActivityId} outside cutoff {Cutoff}.",
                            activity.ActivityId,
                            cutoff
                        );
                        return;
                    }

                    byte[] fitFileData = await this.fitDownloader.DownloadFitFileAsync(
                        integration,
                        activity.ActivityId,
                        token
                    );

                    if (fitFileData.Length == 0)
                    {
                        this.logger.LogWarning(
                            "No FIT data returned for Garmin activity {ActivityId}.",
                            activity.ActivityId
                        );
                        return;
                    }

                    fetchedActivities.Add(
                        new FetchedActivity(
                            ExternalActivityId: activity.ActivityId.ToString(),
                            Source: ServiceTypes.Garmin,
                            ActivityDate: activityStartDate,
                            FileName: $"garmin_{activityStartDate:yyyyMMdd_HHmmss}_{activity.ActivityId}.fit",
                            FitFileData: fitFileData,
                            Metadata: null,
                            ActivityName: activity.ActivityName
                        )
                    );
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    this.logger.LogWarning(
                        ex,
                        "Skipping Garmin activity {ActivityId} for user {UserId}.",
                        activity.ActivityId,
                        integration.UserId
                    );
                }
            }
        );

        this.logger.LogInformation(
            "Processed {Count} Garmin activities for user {UserId}.",
            fetchedActivities.Count,
            integration.UserId
        );

        return [.. fetchedActivities];
    }
}
