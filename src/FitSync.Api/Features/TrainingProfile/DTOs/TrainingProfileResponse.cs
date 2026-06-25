namespace FitSync.Api.Features.TrainingProfile.DTOs;

public record TrainingProfileResponse(
    int? FtpWatts,
    int? CyclingThresholdHr,
    int? CyclingMaxHr,
    int? RunningThresholdHr,
    int? RunningMaxHr,
    int? RunningThresholdPaceSeconds,
    float? PoolLengthMetres,
    int? SwimThresholdHr,
    int? SwimCssSeconds
);
