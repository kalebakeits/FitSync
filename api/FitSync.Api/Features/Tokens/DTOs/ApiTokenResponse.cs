namespace FitSync.Api.Features.Tokens.DTOs;

public record ApiTokenResponse(Guid Id, string Name, DateTime CreatedAt, DateTime? LastUsedAt);
