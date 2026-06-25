namespace FitSync.Shared.Features.WorkoutBuilder.DTOs;

using Dynastream.Fit;

public interface IWorkoutMeta
{
    string Name { get; }
    Sport Sport { get; }
    SubSport? SubSport { get; }
    WorkoutItem[] Items { get; }
    bool SkipLastRest { get; }
}
