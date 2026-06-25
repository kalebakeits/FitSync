namespace FitSync.Shared.Features.WorkoutBuilder.Services.Messages.Swimming;

using Dynastream.Fit;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;
using FitSync.Shared.Features.WorkoutBuilder.Services.Messages.Sports.Base;
using FitSync.Shared.Features.WorkoutBuilder.Services.Messages.WorkoutItem;

public class PoolSwimMessageBuilder(IWorkoutItemResolver resolver)
    : BaseMessageBuilder<WorkoutSchema.PoolSwim>(resolver)
{
    public override WorkoutMesg BuildWorkoutMessage(WorkoutSchema.PoolSwim schema, ushort stepCount)
    {
        WorkoutMesg workoutMesg = base.BuildWorkoutMessage(schema, stepCount);
        workoutMesg.SetPoolLength(schema.PoolLength);
        workoutMesg.SetPoolLengthUnit(schema.PoolLengthUnit);
        return workoutMesg;
    }
}
