namespace FitSync.Mock.Fetcher.Services.WorkoutSeeding;

using FitSync.Shared.Features.WorkoutSchema.Services;
using Microsoft.Extensions.DependencyInjection;

public static class WorkoutSeedingFeature
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddWorkoutSeeding()
        {
            services.AddSingleton<IWorkoutDurationCalculator, WorkoutDurationCalculator>();
            services.AddSingleton<ISeedWorkoutProvider, BikeSeedWorkoutProvider>();
            services.AddSingleton<ISeedWorkoutProvider, RunSeedWorkoutProvider>();
            services.AddSingleton<ISeedWorkoutProvider, SwimSeedWorkoutProvider>();
            services.AddScoped<IWorkoutSeeder, WorkoutSeeder>();
            return services;
        }
    }
}
