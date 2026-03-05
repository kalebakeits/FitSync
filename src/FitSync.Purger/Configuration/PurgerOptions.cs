namespace FitSync.Purger.Configuration;

public class PurgerOptions
{
    public required int PurgeIntervalMinutes { get; set; }

    /// <summary>
    /// Per-source lookback in days. Activities older than lookbackDays+1 are hard-deleted.
    /// Example: { "Zwift": 28, "Wahoo": 180 }
    /// </summary>
    public required Dictionary<string, int> SourceLookbackDays { get; set; }
}
