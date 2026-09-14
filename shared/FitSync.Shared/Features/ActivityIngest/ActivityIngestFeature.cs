namespace FitSync.Shared.Features.ActivityIngest;

using FitSync.Shared.Features.ActivityIngest.Services;
using Microsoft.Extensions.DependencyInjection;

public static class ActivityIngestFeature
{
    public static IServiceCollection AddActivityIngestFeature(this IServiceCollection services)
    {
        services.AddSingleton<IFitSessionDecoder, FitSessionDecoder>();
        services.AddSingleton<ISportCategoryResolver, SportCategoryResolver>();
        services.AddScoped<IScheduledWorkoutMatcher, ScheduledWorkoutMatcher>();
        services.AddScoped<IScheduledWorkoutLinker, ScheduledWorkoutLinker>();
        return services;
    }
}
