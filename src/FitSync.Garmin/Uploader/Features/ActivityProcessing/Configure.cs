namespace FitSync.Garmin.Uploader.Features.ActivityProcessing;

using FitSync.Garmin.Uploader.Features.ActivityProcessing.Services;

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
