namespace FitSync.Wahoo.Shared.WahooClient;

using FitSync.Shared.Features.Fetcher.Services;

public interface IWahooClient : IFetcherClient
{
    Task<byte[]> DownloadFitFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default
    );
}
