namespace FitSync.Api.Features.Credentials.DTOs;

public class CredentialResponse
{
    public required string ServiceType { get; set; }
    public required string Username { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
    public required bool Enabled { get; set; }
}
