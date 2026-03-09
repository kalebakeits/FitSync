namespace FitSync.Garmin.Uploader.Features.GarminUpload.DTOs;

using System.Net;

public class UploadResult
{
    public bool Success { get; init; }
    public bool ShouldRetry { get; init; }
    public HttpStatusCode? StatusCode { get; init; }
    public string? ErrorMessage { get; init; }

    public static UploadResult Succeeded() => new() { Success = true };

    public static UploadResult RateLimited() =>
        new() { ShouldRetry = true, ErrorMessage = "Rate limit reached." };

    public static UploadResult Failed(
        string errorMessage,
        HttpStatusCode? statusCode = null,
        bool shouldRetry = false
    ) =>
        new()
        {
            Success = false,
            ShouldRetry = shouldRetry,
            ErrorMessage = errorMessage,
            StatusCode = statusCode
        };
}
