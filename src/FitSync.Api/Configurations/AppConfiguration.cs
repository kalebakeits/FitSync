namespace FitSync.Api.Configurations;

public class AppConfiguration
{
    public required int MaxSequentialCredentialFailures { get; set; }
    public required int FetcherHeartbeatThresholdMinutes { get; set; }
}
