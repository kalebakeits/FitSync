namespace FitSync.Api.Features.Connections.DTOs;

public enum FetcherStatusReason
{
    None,
    FetcherUnhealthy,
    NoDestinations,
    AllDestinationsUnhealthy,
    SomeDestinationsUnhealthy,
}

public record FetcherStatusResponse(
    string ServiceType,
    /// <summary>"green" | "amber" | "red" | "grey"</summary>
    string Status,
    FetcherStatusReason Reason,
    List<DestinationStatusEntry> Destinations
);
