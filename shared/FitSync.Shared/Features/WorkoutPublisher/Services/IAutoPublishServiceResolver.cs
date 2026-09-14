namespace FitSync.Shared.Features.WorkoutPublisher.Services;

public interface IAutoPublishServiceResolver
{
    Task<List<string>> ResolveServiceTypesAsync(
        Guid userId,
        Guid workoutId,
        CancellationToken cancellationToken = default
    );
}
