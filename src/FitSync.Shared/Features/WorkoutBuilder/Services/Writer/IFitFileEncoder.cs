namespace FitSync.Shared.Features.WorkoutBuilder.Services.Writer;

using Dynastream.Fit;

public interface IFitFileEncoder
{
    byte[] Encode(WorkoutMesg workoutMesg, List<WorkoutStepMesg> workoutSteps);
}
