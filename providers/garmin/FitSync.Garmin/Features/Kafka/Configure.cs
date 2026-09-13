namespace FitSync.Garmin.Features.Kafka;

using FitSync.Garmin.Features.Kafka.Services;

public static class KafkaFeatureExtensions
{
    public static IServiceCollection AddKafkaConsumer(this IServiceCollection services)
    {
        services.AddSingleton<IKafkaConsumer, KafkaConsumer>();

        return services;
    }
}
