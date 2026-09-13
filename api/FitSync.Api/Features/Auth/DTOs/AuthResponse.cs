namespace FitSync.Api.Features.Auth.DTOs;

public record AuthResponse(string SessionId, Guid UserId, string Username);
