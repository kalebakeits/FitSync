namespace FitSync.Api.Configurations;

using System.ComponentModel.DataAnnotations;

public class WorkoutPublishingOptions
{
    [Range(1, 1440)]
    public required int SweepIntervalMinutes { get; set; }

    [Range(1, 1440)]
    public required int PublishDelayMinutes { get; set; }

    [Range(1, 90)]
    public required int PublishHorizonDays { get; set; }

    [Range(1, 10)]
    public required int MaxPublishAttempts { get; set; }
}
