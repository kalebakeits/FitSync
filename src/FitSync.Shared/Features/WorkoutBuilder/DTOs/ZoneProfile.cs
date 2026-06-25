namespace FitSync.Shared.Features.WorkoutBuilder.DTOs;

public record ZoneProfile(
    int? FtpWatts,
    int? CyclingMaxHr,
    int? RunningMaxHr,
    int? SwimMaxHr,
    int? RunningThresholdPaceSeconds,
    int? SwimCssSeconds
);
