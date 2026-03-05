namespace FitSync.Zwift.Shared.ZwiftClient.Services;

using FitSync.Database.Models;
using FitSync.Zwift.Shared.ZwiftClient.DTOs;

public interface IZwiftApiService
{
    Task<ZwiftActivityDto[]> FetchActivitiesAsync(
        Integration integration,
        CancellationToken cancellationToken = default
    );
}
