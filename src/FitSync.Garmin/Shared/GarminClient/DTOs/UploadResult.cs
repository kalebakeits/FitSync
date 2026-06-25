namespace FitSync.Garmin.Shared.GarminClient.DTOs;

using System.Net;

public record UploadResult(
    bool Success,
    bool ShouldRetry,
    HttpStatusCode? StatusCode,
    string? ErrorMessage
)
{
    public static UploadResult Succeeded() => new(true, false, null, null);

    public static UploadResult RateLimited() => new(false, true, null, "Rate limit reached.");

    public static UploadResult Failed(
        string errorMessage,
        HttpStatusCode? statusCode = null,
        bool shouldRetry = false
    ) => new(false, shouldRetry, statusCode, errorMessage);
}
