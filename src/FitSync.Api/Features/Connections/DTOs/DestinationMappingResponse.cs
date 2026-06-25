namespace FitSync.Api.Features.Connections.DTOs;

public record DestinationMappingResponse(
    string SourceServiceType,
    List<string> DestinationServiceTypes
);
