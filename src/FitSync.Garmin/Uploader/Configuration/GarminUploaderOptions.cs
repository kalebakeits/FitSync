namespace FitSync.Garmin.Uploader.Configuration;

using FitSync.Shared.Features.RateLimiting;

public class GarminUploaderOptions
{
    public required string InstanceId { get; set; }
    public required int HeartbeatIntervalMinutes { get; set; }
    public required int MaxRetries { get; set; }
    public required int OrphanThresholdMinutes { get; set; }
    public required List<RateLimit> RateLimits { get; set; }
}
