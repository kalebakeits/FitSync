namespace FitSync.Shared.Features.Kafka.Services;

public interface ITopicInitializer
{
    Task EnsureTopicsExistAsync(
        string? bootstrapServers,
        IReadOnlyList<string> topics,
        CancellationToken cancellationToken
    );
}
