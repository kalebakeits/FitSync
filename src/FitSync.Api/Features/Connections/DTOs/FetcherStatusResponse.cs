namespace FitSync.Api.Features.Connections.DTOs;

public class FetcherStatusResponse
{
    public required string ServiceType { get; set; }
    /// <summary>"green" | "amber" | "red" | "grey"</summary>
    public required string Status { get; set; }
    public required List<DestinationStatusEntry> Destinations { get; set; }
}
