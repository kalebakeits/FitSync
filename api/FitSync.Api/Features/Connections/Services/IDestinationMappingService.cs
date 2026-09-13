namespace FitSync.Api.Features.Connections.Services;

using FitSync.Api.Features.Connections.DTOs;

public interface IDestinationMappingService
{
    Task<List<DestinationMappingResponse>> GetMappingsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task UpsertMappingsAsync(
        Guid userId,
        UpsertDestinationMappingsRequest request,
        CancellationToken cancellationToken = default
    );
}
