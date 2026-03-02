namespace FitSync.Api.Features.Credentials.DTOs;

public class AvailableServiceResponse
{
    public required string ServiceType { get; set; }
    public required string AuthType { get; set; }
    public string? ConnectUrl { get; set; }
}
