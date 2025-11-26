namespace FitSync.ZwiftFetcher.Features.ZwiftClient;

using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.DTOs;

/// <summary>
/// Handles all direct communication and authentication with the Zwift API.
/// </summary>
public interface IZwiftClient
{
    /// <summary>
    /// Fetches a list of recent activities for the given user, handling authentication and download internally.
    /// </summary>
    /// <param name="config">The user's Zwift configuration containing auth tokens and profile ID.</param>
    /// <param name="lookbackDays">The number of days to look back for activities.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of fetched activities ready for persistence.</returns>
    Task<List<FetchedActivity>> GetActivitiesAsync(
        ZwiftFetcherConfig config,
        int lookbackDays,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Authenticates a user with a username and password and populates the config with tokens and profile ID.
    /// </summary>
    /// <param name="config">The config object to populate.</param>
    /// <param name="username">Zwift username.</param>
    /// <param name="password">Zwift password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated ZwiftFetcherConfig.</returns>
    Task AuthenticateAsync(
        ZwiftFetcherConfig config,
        string username,
        string password,
        CancellationToken cancellationToken
    );
}
