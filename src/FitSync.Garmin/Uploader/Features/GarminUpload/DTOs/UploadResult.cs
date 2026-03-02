using System.Net;

namespace FitSync.Garmin.Uploader.Features.GarminUpload.DTOs;

public class UploadResult
{
    public bool Success { get; init; }
    public HttpStatusCode? StatusCode { get; init; }
    public string? ErrorMessage { get; init; }

    public static UploadResult Succeeded() => new() { Success = true };

    public static UploadResult Failed(string errorMessage, HttpStatusCode? statusCode = null) =>
        new()
        {
            Success = false,
            ErrorMessage = errorMessage,
            StatusCode = statusCode
        };
}
