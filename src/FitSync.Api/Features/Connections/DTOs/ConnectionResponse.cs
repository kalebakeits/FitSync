namespace FitSync.Api.Features.Connections.DTOs;

public class ConnectionResponse
{
    public required string ServiceType { get; set; }
    public required string AuthType { get; set; }
    public required bool Connected { get; set; }
    public required bool Enabled { get; set; }
    public string? DisplayName { get; set; }
    public required DateTime UpdatedAt { get; set; }
}
