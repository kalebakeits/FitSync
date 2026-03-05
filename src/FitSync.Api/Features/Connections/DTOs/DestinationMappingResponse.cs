namespace FitSync.Api.Features.Connections.DTOs;

public class DestinationMappingResponse
{
    public required string SourceServiceType { get; set; }
    public required List<string> DestinationServiceTypes { get; set; }
}
