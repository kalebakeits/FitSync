namespace FitSync.Shared.Features.WorkoutPublisher;

using FitSync.Shared.Features.WorkoutPublisher.Services;
using Microsoft.Extensions.DependencyInjection;

public static class WorkoutPublisherFeature
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddWorkoutPublisherFeature()
        {
            services.AddScoped<IScheduledWorkoutPublishService, ScheduledWorkoutPublishService>();
            services.AddScoped<IScheduledWorkoutPushService, ScheduledWorkoutPushService>();
            services.AddScoped<IScheduledWorkoutDeleteService, ScheduledWorkoutDeleteService>();
            services.AddScoped<IWorkoutPushService, WorkoutPushService>();
            services.AddScoped<IWorkoutPushTargetResolver, WorkoutPushTargetResolver>();
            services.AddScoped<IAutoPublishServiceResolver, AutoPublishServiceResolver>();
            return services;
        }
    }
}
