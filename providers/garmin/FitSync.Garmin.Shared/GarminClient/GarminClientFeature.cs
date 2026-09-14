namespace FitSync.Garmin.Shared.GarminClient;

using FitSync.Garmin.Shared.GarminClient.Services;
using FitSync.Shared.Features.WorkoutPublisher.Services;
using Microsoft.Extensions.DependencyInjection;

public static class GarminClientFeature
{
    public static IServiceCollection AddGarminClient(this IServiceCollection services)
    {
        services.AddScoped<IGarminApiClient, GarminApiClient>();
        services.AddScoped<IGarminAuthService, GarminAuthService>();
        services.AddScoped<GarminClient>();
        services.AddScoped<IWorkoutPublisherClient, GarminClient>();
        services.AddScoped<IGarminHttpSender, GarminHttpSender>();
        services.AddScoped<IGarminApiService, GarminApiService>();
        services.AddScoped<IGarminFitDownloader, GarminFitDownloader>();
        services.AddScoped<IGarminActivityProcessor, GarminActivityProcessor>();
        services.AddScoped<IGarminActivityClient, GarminActivityClient>();
        return services;
    }
}
