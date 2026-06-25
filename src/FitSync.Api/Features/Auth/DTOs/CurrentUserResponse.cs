namespace FitSync.Api.Features.Auth.DTOs;

public record CurrentUserResponse(
    Guid UserId,
    string Username,
    string Email,
    bool IsVerified,
    bool IsEmailVerified
);
