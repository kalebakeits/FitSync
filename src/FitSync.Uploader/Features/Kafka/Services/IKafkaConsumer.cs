namespace FitSync.Uploader.Features.Kafka.Services;

public interface IKafkaConsumer
{
    IAsyncEnumerable<string> ConsumeAsync(CancellationToken cancellationToken);
}
