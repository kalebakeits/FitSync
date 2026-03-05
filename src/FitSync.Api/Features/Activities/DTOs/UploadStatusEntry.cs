namespace FitSync.Api.Features.Activities.DTOs;

using FitSync.Database.Enums;

public class UploadStatusEntry
{
    public required string DestinationServiceType { get; set; }
    public required ActivityStatus Status { get; set; }
    public string? LastError { get; set; }
    public int RetryCount { get; set; }
}
