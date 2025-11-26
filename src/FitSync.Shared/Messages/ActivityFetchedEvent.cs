namespace FitSync.Shared.Messages;

public class ActivityFetchedEvent
{
    public Guid ActivityId { get; set; }
    public Guid UserId { get; set; }
    public string ExternalActivityId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime ActivityDate { get; set; }
    public string FileName { get; set; } = string.Empty;
    public byte[] FitFileData { get; set; } = [];
    public Dictionary<string, string> Metadata { get; set; } = [];
    public DateTime FetchedAt { get; set; }
}
