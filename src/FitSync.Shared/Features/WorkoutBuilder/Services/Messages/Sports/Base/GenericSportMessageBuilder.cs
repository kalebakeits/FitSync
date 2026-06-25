namespace FitSync.Shared.Features.WorkoutBuilder.Services.Messages.Sports.Base;

using FitSync.Shared.Features.WorkoutBuilder.DTOs;
using FitSync.Shared.Features.WorkoutBuilder.Services.Messages.WorkoutItem;

public class GenericSportMessageBuilder(IWorkoutItemResolver resolver)
    : BaseMessageBuilder<WorkoutSchema.Default>(resolver);
