namespace FitSync.Shared.Features.ActivityIngest.DTOs;

public record FitSessionStats(
    int? Sport,
    int? DurationSeconds,
    double? DistanceMeters,
    int? AvgHeartRate,
    int? AvgPower
);
