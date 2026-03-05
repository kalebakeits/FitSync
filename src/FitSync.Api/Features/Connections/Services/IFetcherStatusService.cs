namespace FitSync.Api.Features.Connections.Services;

using FitSync.Api.Features.Connections.DTOs;

public interface IFetcherStatusService
{
    Task<List<FetcherStatusResponse>> GetStatusAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
}
