namespace FitSync.Shared.Configuration;

using FitSync.Shared.Features.Fetcher;

/// <summary>
/// Default options for a fetcher.
/// </summary>
public class FetcherOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether this service should run its fetcher at all. When
    /// false the fetcher is never registered, so no worker loop is mounted.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the polling interval for the <see cref="FetcherWorker"/> in minutes.
    /// </summary>
    public required int PollIntervalMinutes { get; set; }

    /// <summary>
    /// Gets or sets the interval in minutes between fetches for each individual user.
    /// </summary>
    public required int FetchIntervalMinutes { get; set; }

    /// <summary>
    /// Gets or sets he number of days in the past to look back when polling user activities.
    /// </summary>
    public required int LookbackDays { get; set; }

    /// <summary>
    /// Gets or sets the instance ID.
    /// </summary>
    public required string InstanceId { get; set; }

    /// <summary>
    /// Gets or sets the heartbeat interval in minutes.
    /// </summary>
    public required int HeartbeatIntervalMinutes { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of users to process in parallel.
    /// </summary>
    public required int MaxParallelUsers { get; set; }

    /// <summary>s
    /// Gets or sets the maximum number of activities to process in parallel per user.
    /// </summary>
    public required int MaxParallelActivities { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of sequential credential failures before a user is skipped.
    /// </summary>
    public required int MaxSequentialCredentialFailures { get; set; }
}
