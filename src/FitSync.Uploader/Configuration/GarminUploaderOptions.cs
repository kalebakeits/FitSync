namespace FitSync.Uploader.Configuration;

public class GarminUploaderOptions
{
    public required string InstanceId { get; set; }
    public required int HeartbeatIntervalMinutes { get; set; }
    public required int MaxRetries { get; set; }
    public required int OrphanThresholdMinutes { get; set; }
}
