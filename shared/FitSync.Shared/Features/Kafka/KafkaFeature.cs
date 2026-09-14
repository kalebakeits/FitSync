namespace FitSync.Shared.Features.Kafka;

using FitSync.Shared.Constants;
using FitSync.Shared.Features.Kafka.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public static class KafkaFeature
{
    public static IServiceCollection AddKafkaTopicInitializer(this IServiceCollection services)
    {
        services.AddSingleton<ITopicInitializer, KafkaTopicInitializer>();

        return services;
    }

    public static async Task EnsureKafkaTopicsAsync(this IHost host)
    {
        ITopicInitializer topicInitializer = host.Services.GetRequiredService<ITopicInitializer>();
        IConfiguration configuration = host.Services.GetRequiredService<IConfiguration>();

        await topicInitializer.EnsureTopicsExistAsync(
            configuration.GetConnectionString("kafka"),
            KafkaTopics.All,
            CancellationToken.None
        );
    }
}
