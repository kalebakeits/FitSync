namespace FitSync.Zwift.Fetcher.Features.ZwiftClient.Services;

using FitSync.Database.Models;
using FitSync.Zwift.Fetcher.Features.ZwiftClient.DTOs;

public interface IZwiftApiService
{
    Task<ZwiftActivityDto[]> FetchActivitiesAsync(
        Integration integration,
        CancellationToken cancellationToken = default
    );
}
