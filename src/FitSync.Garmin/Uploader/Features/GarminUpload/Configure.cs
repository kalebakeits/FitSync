using FitSync.Garmin.Uploader.Features.GarminUpload.Services;

namespace FitSync.Garmin.Uploader.Features.GarminUpload;

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
