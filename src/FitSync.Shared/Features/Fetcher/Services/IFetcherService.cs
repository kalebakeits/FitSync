namespace FitSync.Shared.Features.Fetcher.Services;

using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.DTOs;

public interface IFetcherService
{
    Task<List<FetchedActivity>> GetActivitiesAsync(User user, CancellationToken cancellationToken);
}
