namespace FitSync.Api.Features.Credentials.DTOs;

public record CredentialResponse(
    string ServiceType,
    string Username,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool Enabled
);
