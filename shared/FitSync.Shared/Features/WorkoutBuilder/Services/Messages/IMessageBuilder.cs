namespace FitSync.Shared.Features.WorkoutBuilder.Services.Messages;

using Dynastream.Fit;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public interface IMessageBuilder<in TSchema>
    where TSchema : WorkoutSchema
{
    List<WorkoutStepMesg> BuildWorkoutStepMessages(TSchema schema);
    WorkoutMesg BuildWorkoutMessage(TSchema schema, ushort stepCount);
}
