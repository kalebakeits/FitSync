namespace FitSync.Shared.Features.Kafka.Services;

using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;

public class KafkaTopicInitializer(ILogger<KafkaTopicInitializer> logger) : ITopicInitializer
{
    private const int MaxAttempts = 10;
    private static readonly TimeSpan AttemptTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan FirstRetryDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(5);

    private readonly ILogger<KafkaTopicInitializer> logger = logger;

    public async Task EnsureTopicsExistAsync(
        string? bootstrapServers,
        IReadOnlyList<string> topics,
        CancellationToken cancellationToken
    )
    {
        if (topics.Count == 0)
        {
            this.logger.LogInformation("No Kafka topics to ensure; skipping");
            return;
        }

        if (string.IsNullOrWhiteSpace(bootstrapServers))
        {
            this.logger.LogWarning(
                "Kafka bootstrap servers are not configured; skipping creation of topics {Topics}",
                string.Join(", ", topics)
            );
            return;
        }

        this.logger.LogInformation(
            "Ensuring Kafka topics exist: {Topics}",
            string.Join(", ", topics)
        );

        using IAdminClient adminClient = new AdminClientBuilder(
            new AdminClientConfig { BootstrapServers = bootstrapServers }
        ).Build();

        List<TopicSpecification> specifications = [];
        foreach (string topic in topics)
        {
            specifications.Add(new TopicSpecification { Name = topic });
        }

        TimeSpan retryDelay = FirstRetryDelay;

        for (int attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await adminClient.CreateTopicsAsync(
                    specifications,
                    new CreateTopicsOptions
                    {
                        RequestTimeout = AttemptTimeout,
                        OperationTimeout = AttemptTimeout,
                    }
                );

                this.logger.LogInformation(
                    "Created Kafka topics: {Topics}",
                    string.Join(", ", topics)
                );
                return;
            }
            catch (CreateTopicsException ex)
            {
                List<string> created = [];
                List<string> existing = [];
                List<string> unreachable = [];

                foreach (CreateTopicReport report in ex.Results)
                {
                    if (report.Error.Code == ErrorCode.TopicAlreadyExists)
                    {
                        existing.Add(report.Topic);
                    }
                    else if (report.Error.Code == ErrorCode.NoError)
                    {
                        created.Add(report.Topic);
                    }
                    else if (Convert.ToInt32(report.Error.Code) < 0)
                    {
                        // Negative codes are librdkafka client-side failures (broker not
                        // reachable yet), not the broker rejecting the topic — retry those.
                        unreachable.Add($"{report.Topic}: {report.Error.Reason}");
                    }
                    else
                    {
                        this.logger.LogError(
                            "Failed to create Kafka topic {Topic}: {Reason}",
                            report.Topic,
                            report.Error.Reason
                        );
                        throw;
                    }
                }

                if (unreachable.Count == 0)
                {
                    this.logger.LogInformation(
                        "Kafka topics already existed: {Existing}; created: {Created}",
                        string.Join(", ", existing),
                        string.Join(", ", created)
                    );
                    return;
                }

                this.logger.LogWarning(
                    "Kafka is not reachable yet (attempt {Attempt}/{MaxAttempts}): {Errors}",
                    attempt,
                    MaxAttempts,
                    string.Join(", ", unreachable)
                );
            }
            catch (KafkaException ex)
            {
                this.logger.LogWarning(
                    ex,
                    "Kafka is not reachable yet (attempt {Attempt}/{MaxAttempts}): {Reason}",
                    attempt,
                    MaxAttempts,
                    ex.Error.Reason
                );
            }

            if (attempt < MaxAttempts)
            {
                await Task.Delay(retryDelay, cancellationToken);
                TimeSpan nextRetryDelay = retryDelay * 2;
                retryDelay = nextRetryDelay > MaxRetryDelay ? MaxRetryDelay : nextRetryDelay;
            }
        }

        this.logger.LogError(
            "Kafka topics {Topics} still missing after {MaxAttempts} attempts; starting anyway",
            string.Join(", ", topics),
            MaxAttempts
        );
    }
}
