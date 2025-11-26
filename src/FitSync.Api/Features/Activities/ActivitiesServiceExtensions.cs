namespace FitSync.Api.Features.Activities;

using FitSync.Api.Features.Activities.Services;

public static class ActivitiesServiceExtensions
{
    public static IServiceCollection AddActivitiesFeature(this IServiceCollection services)
    {
        services.AddScoped<IActivitiesService, ActivitiesService>();
        return services;
    }
}
