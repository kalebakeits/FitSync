namespace FitSync.ZwiftFetcher.Features.ZwiftClient.Services;

using FitSync.Database.Models;

public interface IZwiftAuthService
{
    Task EnsureAuthenticatedAsync(
        ZwiftFetcherConfig config,
        CancellationToken cancellationToken = default
    );
    Task AuthenticateAsync(
        ZwiftFetcherConfig config,
        string username,
        string password,
        CancellationToken cancellationToken = default
    );
    Task<bool> TryRefreshOrReauthenticateAsync(
        ZwiftFetcherConfig config,
        CancellationToken cancellationToken = default
    );
}
