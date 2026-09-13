namespace FitSync.Garmin.Features.Kafka.Services;

public interface IKafkaConsumer
{
    IAsyncEnumerable<string> ConsumeAsync(CancellationToken cancellationToken);
}
