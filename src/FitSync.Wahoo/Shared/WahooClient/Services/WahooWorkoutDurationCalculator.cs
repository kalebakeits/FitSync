namespace FitSync.Wahoo.Shared.WahooClient.Services;

using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public class WahooWorkoutDurationCalculator : IWahooWorkoutDurationCalculator
{
    public int CalculateMinutes(WorkoutItem[] items)
    {
        return Math.Max(1, (int)(SumMs(items) / 60000));
    }

    public long SumMs(WorkoutItem[] items)
    {
        long total = 0;
        foreach (WorkoutItem item in items)
        {
            item.Match(
                step => total += step.DurationValue ?? 0,
                _ => { },
                repeat => total += SumMs(repeat.Steps) * repeat.RepeatCount
            );
        }
        return total;
    }
}
