namespace FitSync.Api.Features.WorkoutPublishing;

using FitSync.Api.Features.WorkoutPublishing.Services;

public static class WorkoutPublishingFeature
{
    public static IServiceCollection AddWorkoutPublishingFeature(this IServiceCollection services)
    {
        services.AddScoped<IWorkoutPublishingService, WorkoutPublishingService>();
        return services;
    }
}
