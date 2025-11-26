namespace FitSync.Api.Features.Auth.DTOs;

public class CurrentUserResponse
{
    public required Guid UserId { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required bool IsVerified { get; set; }
    public required bool IsEmailVerified { get; set; }
}
