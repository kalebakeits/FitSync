namespace FitSync.Api.Features.Connections.DTOs;

public record ConnectionResponse(
    string ServiceType,
    string AuthType,
    bool Connected,
    bool Enabled,
    string? DisplayName,
    DateTime UpdatedAt
);
