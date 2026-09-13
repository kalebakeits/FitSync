namespace FitSync.Wahoo.Shared.WahooClient.Services;

using FitSync.Shared.Features.Fetcher.DTOs;
using FitSync.Wahoo.Shared.WahooClient.DTOs;

public interface IWahooActivityProcessor
{
    Task<List<FetchedActivity>> ProcessActivitiesAsync(
        List<WahooWorkoutDto> workouts,
        CancellationToken cancellationToken = default
    );

    Task<byte[]> DownloadFitFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default
    );
}
