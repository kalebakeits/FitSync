namespace FitSync.Api.Features.Credentials.DTOs;

public record AvailableServiceResponse(
    string ServiceType,
    string AuthType,
    string? ConnectUrl,
    bool IsFetcher,
    bool IsUploader
);
