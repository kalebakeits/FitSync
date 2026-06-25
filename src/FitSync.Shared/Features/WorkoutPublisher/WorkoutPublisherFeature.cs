namespace FitSync.Shared.Features.WorkoutPublisher;

using FitSync.Shared.Features.WorkoutPublisher.Services;
using Microsoft.Extensions.DependencyInjection;

public static class WorkoutPublisherFeature
{
    extension (IServiceCollection services)
    {
        public IServiceCollection AddWorkoutPublisherFeature()
        {
            services.AddScoped<IWorkoutPublisherService, WorkoutPublisherService>();
            return services;
        }
    }
}
