namespace FitSync.Wahoo.Fetcher.Configuration;

using FitSync.Shared.Configuration;
using FitSync.Wahoo.Shared.Configuration;

public class WahooFetcherOptions : WahooClientOptions
{
    public required int PollIntervalMinutes { get; set; }
    public required int LookbackDays { get; set; }
    public required int DeadThresholdMinutes { get; set; }
    public required int MaxPendingActivities { get; set; }
    public required string InstanceId { get; set; }
    public required int HeartbeatIntervalMinutes { get; set; }
    public required int MaxParallelUsers { get; set; }
    public required int MaxParallelActivities { get; set; }
    public required int MaxSequentialCredentialFailures { get; set; }
}
