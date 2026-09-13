namespace FitSync.Garmin.Features.GarminUpload;

using FitSync.Garmin.Shared.GarminClient;
using FitSync.Garmin.Shared.GarminClient.Services;
using FitSync.Garmin.Features.GarminUpload.Services;

public static class GarminUploadFeatureExtensions
{
    public static IServiceCollection AddGarminUpload(this IServiceCollection services)
    {
        services.AddScoped<IGarminApiClient, GarminApiClient>();
        services.AddScoped<IGarminAuthService, GarminAuthService>();
        services.AddScoped<IGarminUploader, GarminUploader>();

        return services;
    }
}
