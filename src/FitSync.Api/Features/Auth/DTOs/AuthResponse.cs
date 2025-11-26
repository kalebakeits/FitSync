namespace FitSync.Api.Features.Auth.DTOs;

public class AuthResponse
{
    public required string SessionId { get; set; }
    public required Guid UserId { get; set; }
    public required string Username { get; set; }
}
