namespace FitSync.Garmin.Configuration;

using FitSync.Shared.Features.RateLimiting;

public class GarminUploaderOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether this service should run its uploader at all. When
    /// false no uploader worker is registered, so no consume loop is mounted.
    /// </summary>
    public bool Enabled { get; set; } = true;

    public required string InstanceId { get; set; }
    public required int HeartbeatIntervalMinutes { get; set; }
    public required int MaxRetries { get; set; }
    public required int OrphanThresholdMinutes { get; set; }
    public required List<RateLimit> RateLimits { get; set; }
}
