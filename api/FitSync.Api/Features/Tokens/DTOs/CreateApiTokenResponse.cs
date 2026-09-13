namespace FitSync.Api.Features.Tokens.DTOs;

public record CreateApiTokenResponse(Guid Id, string Name, string Token, DateTime CreatedAt);
