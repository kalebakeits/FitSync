namespace FitSync.Shared.Features.Fetcher.Services;

using System.Text.Json;
using Confluent.Kafka;
using FitSync.Database.Models;
using FitSync.Shared.Constants;
using FitSync.Shared.Messages;
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
        ActivityFetchedEvent message =
            new()
            {
                ActivityId = activity.Id,
                UserId = activity.UserId,
                ExternalActivityId = activity.ExternalActivityId,
                Source = activity.Source,
                ActivityDate = activity.ActivityDate,
                FileName = activity.OriginalFileName ?? "unknown.fit",
                FitFileData = activity.FitFileData ?? [],
                Metadata = ParseMetadata(activity.ActivityMetadata),
                FetchedAt = DateTime.UtcNow
            };

        string json = JsonSerializer.Serialize(message);
        Message<string, string> kafkaMessage = new() { Key = activity.Id.ToString(), Value = json };

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

    private static Dictionary<string, string> ParseMetadata(string? metadataJson)
    {
        if (string.IsNullOrEmpty(metadataJson))
            return [];

        Dictionary<string, object>? metadata = JsonSerializer.Deserialize<
            Dictionary<string, object>
        >(metadataJson);
        return metadata?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? string.Empty)
            ?? [];
    }
}
