namespace FitSync.Shared.Features.Fetcher;

using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.Services;
using Microsoft.Extensions.Logging;

public abstract class UserQueuerServiceBase(
    IDestinationGate destinationGate,
    ILogger logger
) : IUserQueuerService
{
    private readonly IDestinationGate destinationGate = destinationGate;
    private readonly ILogger logger = logger;

    protected abstract string SourceServiceType { get; }

    public async Task<User[]> GetDueUsersAsync()
    {
        List<Guid> candidates = await this.GetCandidateUserIdsAsync();

        if (candidates.Count == 0)
            return [];

        List<Guid> eligible = await this.destinationGate.FilterEligibleAsync(
            this.SourceServiceType,
            candidates
        );

        if (eligible.Count == 0)
        {
            this.logger.LogInformation(
                "No eligible {Source} users after destination gate check.",
                this.SourceServiceType
            );
            return [];
        }

        return await this.ClaimUsersAsync(eligible);
    }

    public abstract Task<bool> ReleaseUsersAsync(User[] users);

    /// <summary>Returns user IDs of candidates ready for fetch (schedule-based, before destination gating).</summary>
    protected abstract Task<List<Guid>> GetCandidateUserIdsAsync();

    /// <summary>Acquires the lock for the eligible user IDs and returns the corresponding User objects.</summary>
    protected abstract Task<User[]> ClaimUsersAsync(List<Guid> eligibleUserIds);
}
