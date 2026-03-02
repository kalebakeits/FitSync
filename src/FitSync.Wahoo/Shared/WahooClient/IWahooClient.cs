namespace FitSync.Wahoo.Shared.WahooClient;

using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.DTOs;

public interface IWahooClient
{
    Task<List<FetchedActivity>> GetActivitiesAsync(
        Integration integration,
        int lookbackDays,
        CancellationToken cancellationToken = default
    );

    Task<byte[]> DownloadFitFileAsync(string fileUrl, CancellationToken cancellationToken = default);
}
