namespace FitSync.Api.Features.Connections.Services;

using FitSync.Api.Features.Connections.DTOs;

public interface IConnectionsService
{
    Task<List<ConnectionResponse>> GetConnectionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task DisconnectAsync(
        Guid userId,
        string serviceType,
        CancellationToken cancellationToken = default
    );
}
