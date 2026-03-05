namespace FitSync.Api.Features.Connections.DTOs;

public class DestinationStatusEntry
{
    public required string ServiceType { get; set; }
    public bool Healthy { get; set; }
    public bool Connected { get; set; }
}
