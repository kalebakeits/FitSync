namespace FitSync.Api.Features.Activities.DTOs;

public record ActivityResponse(
    Guid Id,
    string ExternalActivityId,
    string Source,
    string? OriginalFileName,
    long? FileSizeBytes,
    DateTime ActivityDate,
    string? ActivityName,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<UploadStatusEntry> UploadStatuses
);
