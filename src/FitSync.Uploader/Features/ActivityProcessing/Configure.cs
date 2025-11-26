using FitSync.Uploader.Features.ActivityProcessing.Services;

namespace FitSync.Uploader.Features.ActivityProcessing;

public static class ActivityProcessingFeatureExtensions
{
    public static IServiceCollection AddActivityProcessing(this IServiceCollection services)
    {
        services.AddScoped<IActivityProcessor, ActivityProcessor>();
        services.AddScoped<IActivityConsumer, ActivityConsumer>();
        services.AddScoped<IActivityStatusMapper, ActivityStatusMapper>();
        services.AddScoped<IUploadResultHandler, UploadResultHandler>();

        services.AddHostedService<ActivityConsumerWorker>();

        return services;
    }
}
