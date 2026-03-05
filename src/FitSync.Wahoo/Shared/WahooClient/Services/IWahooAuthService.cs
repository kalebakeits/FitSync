namespace FitSync.Wahoo.Shared.WahooClient.Services;

using FitSync.Database.Models;

public interface IWahooAuthService
{
    Task EnsureAuthenticatedAsync(Integration integration, CancellationToken cancellationToken = default);
}
