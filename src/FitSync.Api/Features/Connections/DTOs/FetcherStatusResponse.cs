namespace FitSync.Api.Features.Connections.DTOs;

public enum FetcherStatusReason
{
    None,
    FetcherUnhealthy,
    NoDestinations,
    AllDestinationsUnhealthy,
    SomeDestinationsUnhealthy,
}

public class FetcherStatusResponse
{
    public required string ServiceType { get; set; }

    /// <summary>"green" | "amber" | "red" | "grey"</summary>
    public required string Status { get; set; }
    public required FetcherStatusReason Reason { get; set; }
    public required List<DestinationStatusEntry> Destinations { get; set; }
}
