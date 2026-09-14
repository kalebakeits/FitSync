namespace FitSync.Garmin.Shared.GarminClient.Services;

using FitSync.Database.Models;

public interface IGarminFitDownloader
{
    Task<byte[]> DownloadFitFileAsync(
        Integration integration,
        long activityId,
        CancellationToken cancellationToken = default
    );
}
