namespace FitSync.Api.Features.Activities.DTOs;

using FitSync.Database.Enums;

public record UploadStatusEntry(
    string DestinationServiceType,
    ActivityStatus Status,
    string? LastError,
    int RetryCount
);
