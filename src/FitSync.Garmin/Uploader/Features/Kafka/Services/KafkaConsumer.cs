namespace FitSync.Garmin.Uploader.Features.Kafka.Services;

using System.Text.Json;
using Confluent.Kafka;
using FitSync.Shared.Constants;

public class KafkaConsumer : IKafkaConsumer, IDisposable
{
    private readonly IConsumer<string, string> consumer;
    private readonly ILogger<KafkaConsumer> logger;

    public KafkaConsumer(IConsumer<string, string> consumer, ILogger<KafkaConsumer> logger)
    {
        this.consumer = consumer;
        this.logger = logger;

        this.consumer.Subscribe(KafkaTopics.ActivityFetched);

        this.logger.LogInformation(
            "Kafka consumer initialized and subscribed to topic: {Topic}",
            KafkaTopics.ActivityFetched
        );
    }

    public async IAsyncEnumerable<string> ConsumeAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            ConsumeResult<string, string>? result = null;
            string? message = null;

            try
            {
                result = this.consumer.Consume(TimeSpan.FromSeconds(1));

                if (result == null)
                    continue;

                message = result.Message.Value;
                if (message == null)
                {
                    this.logger.LogWarning(
                        "Failed to deserialize message at offset {Offset}",
                        result.Offset
                    );
                    this.consumer.Commit(result);
                    continue;
                }
            }
            catch (ConsumeException ex)
            {
                this.logger.LogError(ex, "Kafka consume error");
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                continue;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error processing Kafka message");
                if (result != null)
                {
                    this.consumer.Commit(result);
                }
                continue;
            }

            // Yield outside of try-catch
            if (message != null)
            {
                yield return message;

                // Commit after successful processing
                if (result != null)
                {
                    this.consumer.Commit(result);
                }
            }
        }
    }

    public void Dispose()
    {
        try
        {
            this.consumer?.Close();
            this.consumer?.Dispose();
            this.logger.LogInformation("Kafka consumer disposed");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error disposing Kafka consumer");
        }
    }
}
