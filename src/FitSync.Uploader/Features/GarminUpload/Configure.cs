using FitSync.Uploader.Features.GarminUpload.Services;

namespace FitSync.Uploader.Features.GarminUpload;

public static class GarminUploadFeatureExtensions
{
    public static IServiceCollection AddGarminUpload(this IServiceCollection services)
    {
        services.AddScoped<IGarminUploader, GarminUploader>();

        return services;
    }
}
