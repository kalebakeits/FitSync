namespace FitSync.Api.Features.Activities.DTOs;

public class ActivityResponse
{
    public required Guid Id { get; set; }
    public required string ExternalActivityId { get; set; }
    public required string Source { get; set; }
    public string? OriginalFileName { get; set; }
    public long? FileSizeBytes { get; set; }
    public required DateTime ActivityDate { get; set; }
    public string? ActivityName { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
    public List<UploadStatusEntry> UploadStatuses { get; set; } = [];
}
