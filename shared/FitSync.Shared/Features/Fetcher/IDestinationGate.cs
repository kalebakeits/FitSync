namespace FitSync.Shared.Features.Fetcher;

public interface IDestinationGate
{
    Task<List<Guid>> FilterEligibleAsync(
        string sourceServiceType,
        List<Guid> userIds,
        CancellationToken cancellationToken = default
    );
}
