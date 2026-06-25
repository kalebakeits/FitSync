namespace FitSync.Api.Features.Connections.DTOs;

public record DestinationStatusEntry(string ServiceType, bool Healthy, bool Connected);
