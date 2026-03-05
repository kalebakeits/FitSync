using FitSync.Database.Models;

namespace FitSync.Garmin.Uploader.Features.GarminUpload.Services;

public interface IGarminAuthService
{
    Task EnsureAuthenticatedAsync(Integration integration, CancellationToken ct);
    Task<bool> TryRefreshAsync(Integration integration, CancellationToken ct);
}
