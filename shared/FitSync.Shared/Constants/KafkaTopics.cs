namespace FitSync.Shared.Constants;

public static class KafkaTopics
{
    public const string ActivityFetched = "fitsync.activity.fetched";

    public static IReadOnlyList<string> All { get; } = [ActivityFetched];
}
