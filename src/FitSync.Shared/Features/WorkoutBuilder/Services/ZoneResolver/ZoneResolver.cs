namespace FitSync.Shared.Features.WorkoutBuilder.Services.ZoneResolver;

using Dynastream.Fit;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public class ZoneResolver : IZoneResolver
{
    // Standard 7-zone power model: (low%, high%) as fraction of FTP
    private static readonly (double Low, double High)[] PowerZones =
    [
        (0.00, 0.55), // Z1
        (0.55, 0.75), // Z2
        (0.75, 0.90), // Z3
        (0.90, 1.05), // Z4
        (1.05, 1.20), // Z5
        (1.20, 1.50), // Z6
        (1.50, 2.00), // Z7
    ];

    // Standard 5-zone HR model: (low%, high%) as fraction of max HR
    private static readonly (double Low, double High)[] HrZones =
    [
        (0.00, 0.60), // Z1
        (0.60, 0.70), // Z2
        (0.70, 0.80), // Z3
        (0.80, 0.90), // Z4
        (0.90, 1.00), // Z5
    ];

    // Pace zones as multiplier of threshold pace (seconds/km or seconds/100m).
    // Pace is inverse — a lower number is faster.
    // Multiplier > 1.0 = slower than threshold, multiplier < 1.0 = faster.
    private static readonly (double LowMultiplier, double HighMultiplier)[] PaceZones =
    [
        (1.35, 2.00), // Z1 — Recovery: 35-100% slower than threshold pace
        (1.15, 1.34), // Z2 — Easy: 15-34% slower
        (1.01, 1.14), // Z3 — Tempo: 1-14% slower
        (0.95, 1.00), // Z4 — Threshold: 0-5% faster to threshold
        (0.80, 0.94), // Z5 — VO2Max/Interval: 6-20% faster
    ];

    // Default pace values used when the user hasn't set them
    private const int DefaultRunningThresholdPaceSeconds = 270; // 4:30/km
    private const int DefaultSwimCssSeconds = 105; // 1:45/100m

    public (uint Low, uint High)? Resolve(
        WktStepTarget targetType,
        Sport sport,
        int zone,
        ZoneProfile profile
    )
    {
        if (IsPowerTarget(targetType))
            return this.ResolvePowerZone(zone, profile.FtpWatts);

        if (targetType == WktStepTarget.HeartRate)
            return this.ResolveHrZone(zone, sport, profile);

        if (targetType is WktStepTarget.Speed or WktStepTarget.SpeedLap)
            return this.ResolvePaceZone(zone, sport, profile);

        return null;
    }

    private static bool IsPowerTarget(WktStepTarget t) =>
        t
            is WktStepTarget.Power
                or WktStepTarget.Power3s
                or WktStepTarget.Power10s
                or WktStepTarget.Power30s
                or WktStepTarget.PowerLap;

    private (uint Low, uint High)? ResolvePowerZone(int zone, int? ftpWatts)
    {
        if (ftpWatts is null || zone < 1 || zone > PowerZones.Length)
            return null;

        (double lowPct, double highPct) = PowerZones[zone - 1];
        return ((uint)(ftpWatts.Value * lowPct), (uint)(ftpWatts.Value * highPct));
    }

    private (uint Low, uint High)? ResolveHrZone(int zone, Sport sport, ZoneProfile profile)
    {
        int? maxHr = sport switch
        {
            Sport.Running => profile.RunningMaxHr,
            Sport.Swimming => profile.SwimMaxHr,
            _ => profile.CyclingMaxHr,
        };

        if (maxHr is null || zone < 1 || zone > HrZones.Length)
            return null;

        (double lowPct, double highPct) = HrZones[zone - 1];
        return ((uint)(maxHr.Value * lowPct), (uint)(maxHr.Value * highPct));
    }

    private (uint Low, uint High)? ResolvePaceZone(int zone, Sport sport, ZoneProfile profile)
    {
        if (zone < 1 || zone > PaceZones.Length)
            return null;

        int thresholdSecondsPerUnit = sport switch
        {
            Sport.Running
                => profile.RunningThresholdPaceSeconds ?? DefaultRunningThresholdPaceSeconds,
            Sport.Swimming => profile.SwimCssSeconds ?? DefaultSwimCssSeconds,
            _ => DefaultRunningThresholdPaceSeconds,
        };

        (double lowMultiplier, double highMultiplier) = PaceZones[zone - 1];

        // Pace zones: lower multiplier = faster (fewer seconds).
        // HighMultiplier is the slower bound (more seconds), LowMultiplier is the faster bound (fewer seconds).
        // FIT speed is in m/s, so convert threshold pace (s/km or s/100m) to m/s first.
        double thresholdMetersPerSecond =
            sport == Sport.Running
                ? 1000.0 / thresholdSecondsPerUnit
                : 100.0 / thresholdSecondsPerUnit;

        uint slowSpeed = (uint)(thresholdMetersPerSecond / highMultiplier);
        uint fastSpeed = (uint)(thresholdMetersPerSecond / lowMultiplier);

        return (fastSpeed, slowSpeed);
    }
}
