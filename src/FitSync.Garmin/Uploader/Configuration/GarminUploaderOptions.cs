namespace FitSync.Garmin.Uploader.Configuration;

public class GarminUploaderOptions
{
    public required string InstanceId { get; set; }
    public required int HeartbeatIntervalMinutes { get; set; }
    public required int MaxRetries { get; set; }
    public required int OrphanThresholdMinutes { get; set; }
    public required int GarminApiRateLimit { get; set; }
}
