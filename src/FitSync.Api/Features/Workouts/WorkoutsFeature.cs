namespace FitSync.Api.Features.Workouts;

using FitSync.Api.Features.Workouts.Services;

public static class WorkoutsFeature
{
    public static IServiceCollection AddWorkoutsFeature(this IServiceCollection services)
    {
        services.AddScoped<IWorkoutsService, WorkoutsService>();
        return services;
    }
}
