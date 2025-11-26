using FitSync.Shared.Messages;

namespace FitSync.Uploader.Features.Kafka.Services;

public interface IKafkaConsumer
{
    IAsyncEnumerable<ActivityFetchedEvent> ConsumeAsync(CancellationToken cancellationToken);
}
