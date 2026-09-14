namespace FitSync.Shared.Features.WorkoutSchema.Services;

using Dynastream.Fit;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public class WorkoutDurationCalculator : IWorkoutDurationCalculator
{
    public int? CalculateSeconds(WorkoutSchema schema)
    {
        int seconds = schema.Match(
            @default => this.CalculateSeconds(@default.Items),
            poolSwim => this.CalculateSeconds(poolSwim.Items)
        );

        return seconds > 0 ? seconds : null;
    }

    public int CalculateSeconds(WorkoutItem[] items) =>
        items.Sum(item =>
            item.Match<int>(
                step =>
                    step.DurationType == WktStepDuration.Time && step.DurationValue.HasValue
                        ? Convert.ToInt32(step.DurationValue.Value / 1000)
                        : 0,
                _ => 0,
                repeat => this.CalculateSeconds(repeat.Steps) * Convert.ToInt32(repeat.RepeatCount)
            )
        );
}
