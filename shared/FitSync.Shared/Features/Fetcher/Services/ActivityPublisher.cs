namespace FitSync.Shared.Features.Fetcher.Services;

using System.Text.Json;
using Confluent.Kafka;
using FitSync.Database.Models;
using FitSync.Shared.Constants;
using Microsoft.Extensions.Logging;

public class ActivityPublisher(
    IProducer<string, string> producer,
    ILogger<ActivityPublisher> logger
) : IActivityPublisher
{
    private readonly IProducer<string, string> producer = producer;
    private readonly ILogger<ActivityPublisher> logger = logger;

    public async Task PublishActivityFetchedAsync(
        Activity activity,
        CancellationToken cancellationToken
    )
    {
        Message<string, string> kafkaMessage =
            new() { Key = activity.Id.ToString(), Value = activity.Id.ToString() };

        DeliveryResult<string, string> result = await this.producer.ProduceAsync(
            KafkaTopics.ActivityFetched,
            kafkaMessage,
            cancellationToken
        );

        this.logger.LogInformation(
            "Published activity {ActivityId} to Kafka at offset {Offset}",
            activity.Id,
            result.Offset
        );
    }
}
