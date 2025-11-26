namespace FitSync.Api.Features.Auth.DTOs;

public class AuthSuccessResponse
{
    public required Guid UserId { get; set; }
    public required string Username { get; set; }
}
