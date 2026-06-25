namespace FitSync.Shared.Features.WorkoutBuilder.Services.Messages.Sports.Base;

using Dynastream.Fit;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;
using FitSync.Shared.Features.WorkoutBuilder.Services.Messages.WorkoutItem;

public abstract class BaseMessageBuilder<TSchema>(IWorkoutItemResolver resolver)
    : IMessageBuilder<TSchema>
    where TSchema : WorkoutSchema, IWorkoutMeta
{
    private readonly IWorkoutItemResolver resolver = resolver;

    public virtual WorkoutMesg BuildWorkoutMessage(TSchema schema, ushort stepCount)
    {
        WorkoutMesg workoutMesg = new();
        workoutMesg.SetWktName(schema.Name);
        workoutMesg.SetSport(schema.Sport);
        workoutMesg.SetSubSport(schema.SubSport ?? SubSport.Generic);
        workoutMesg.SetNumValidSteps(stepCount);
        return workoutMesg;
    }

    public virtual List<WorkoutStepMesg> BuildWorkoutStepMessages(TSchema schema)
    {
        List<WorkoutStepMesg> steps = [];
        foreach (WorkoutItem item in schema.Items)
            this.resolver.Resolve(item, steps, schema.Sport);
        return steps;
    }
}
