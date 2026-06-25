namespace FitSync.Wahoo.Shared.WahooClient;

using FitSync.Shared.Features.Fetcher.Services;
using FitSync.Shared.Features.WorkoutPublisher.Services;

public interface IWahooClient : IFetcherClient, IWorkoutPublisherClient
{
    Task<byte[]> DownloadFitFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default
    );
}
