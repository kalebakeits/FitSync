namespace FitSync.Garmin.Uploader.Features.Kafka.Services;

public interface IKafkaConsumer
{
    IAsyncEnumerable<string> ConsumeAsync(CancellationToken cancellationToken);
}
