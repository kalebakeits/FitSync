namespace FitSync.Shared.Features.Fetcher.DTOs;

public record FetchedActivity(
    string ExternalActivityId,
    string Source,
    DateTime ActivityDate,
    string FileName,
    byte[] FitFileData,
    Dictionary<string, string>? Metadata = null,
    string? ActivityName = null
);
