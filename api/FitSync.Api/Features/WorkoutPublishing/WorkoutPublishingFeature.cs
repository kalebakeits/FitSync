namespace FitSync.Api.Features.WorkoutPublishing;

using FitSync.Api.Features.WorkoutPublishing.Services;
using FitSync.Api.Features.WorkoutPublishing.Workers;

public static class WorkoutPublishingFeature
{
    public static IServiceCollection AddWorkoutPublishingFeature(this IServiceCollection services)
    {
        services.AddScoped<IScheduledWorkoutResponseFactory, ScheduledWorkoutResponseFactory>();
        services.AddScoped<IWorkoutPublishingService, WorkoutPublishingService>();
        services.AddScoped<IScheduledWorkoutPublishDeferral, ScheduledWorkoutPublishDeferral>();
        services.AddScoped<IPendingPublishClaimService, PendingPublishClaimService>();
        services.AddScoped<IPendingPublishSweeper, PendingPublishSweeper>();
        services.AddHostedService<PendingPublishSweeperWorker>();
        return services;
    }
}
