namespace FitSync.Garmin.Uploader.Features.Kafka;

using FitSync.Garmin.Uploader.Features.Kafka.Services;

public static class KafkaFeatureExtensions
{
    public static IServiceCollection AddKafkaConsumer(this IServiceCollection services)
    {
        services.AddSingleton<IKafkaConsumer, KafkaConsumer>();

        return services;
    }
}
