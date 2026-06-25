namespace FitSync.Garmin.Shared.GarminClient.Services;

using FitSync.Database.Models;

public interface IGarminAuthService
{
    Task EnsureAuthenticatedAsync(Integration integration, CancellationToken ct);
    Task<bool> TryRefreshAsync(Integration integration, CancellationToken ct);
}
