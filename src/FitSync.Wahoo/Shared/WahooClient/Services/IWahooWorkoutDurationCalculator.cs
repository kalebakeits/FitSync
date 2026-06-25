namespace FitSync.Wahoo.Shared.WahooClient.Services;

using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public interface IWahooWorkoutDurationCalculator
{
    int CalculateMinutes(WorkoutItem[] items);
}
