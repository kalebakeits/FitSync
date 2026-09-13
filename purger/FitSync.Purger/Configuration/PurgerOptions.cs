namespace FitSync.Purger.Configuration;

public class PurgerOptions
{
    public required int PurgeIntervalMinutes { get; set; }

    /// <summary>
    /// Per-source soft-delete lookback in days. Soft-deleted activities older than lookbackDays+1
    /// are hard-deleted. This purge always runs — required for compliance.
    /// Example: { "Zwift": 28, "Wahoo": 180 }
    /// </summary>
    public required Dictionary<string, int> SourceLookbackDays { get; set; }

    /// <summary>
    /// Optional data-retention purge. When enabled, all activities (regardless of soft-delete)
    /// older than DataRetentionDays are hard-deleted. Use to reclaim DB space on demand.
    /// </summary>
    public bool EnableDataRetentionPurge { get; set; } = false;

    public int DataRetentionDays { get; set; } = 365;
}
