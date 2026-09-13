namespace FitSync.Shared.Features.WorkoutBuilder.Services.WahooWorkoutBuilder;

using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public interface IWahooWorkoutBuilder
{
    WahooPlanDto Build(WorkoutSchema schema);
}
