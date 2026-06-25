namespace FitSync.Shared.Features.WorkoutBuilder.Services.SchemaResolver;

using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public interface IWorkoutSchemaResolver
{
    WorkoutSchema Resolve(WorkoutSchema schema, ZoneProfile profile);
}
