namespace FitSync.ZwiftFetcher.Features.ZwiftClient.Services;

using FitSync.Database.Models;
using FitSync.ZwiftFetcher.Features.ZwiftClient.DTOs;

public interface IZwiftApiService
{
    Task<ZwiftActivityDto[]> FetchActivitiesAsync(
        ZwiftFetcherConfig config,
        CancellationToken cancellationToken = default
    );
}
