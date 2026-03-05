namespace FitSync.Zwift.Shared.ZwiftClient.Services;

using FitSync.Database.Models;

public interface IZwiftAuthService
{
    Task EnsureAuthenticatedAsync(
        Integration integration,
        CancellationToken cancellationToken = default
    );

    Task<bool> TryRefreshOrReauthenticateAsync(
        Integration integration,
        CancellationToken cancellationToken = default
    );
}
