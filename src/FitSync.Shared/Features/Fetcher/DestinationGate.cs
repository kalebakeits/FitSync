namespace FitSync.Shared.Features.Fetcher;

using FitSync.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class DestinationGate(
    FitSyncDbContext dbContext,
    ILogger<DestinationGate> logger
) : IDestinationGate
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly ILogger<DestinationGate> logger = logger;

    public async Task<List<Guid>> FilterEligibleAsync(
        string sourceServiceType,
        List<Guid> userIds,
        CancellationToken cancellationToken = default
    )
    {
        if (userIds.Count == 0)
            return [];

        var mappings = await this.dbContext.UserDestinationConfigs
            .Where(c => userIds.Contains(c.UserId) && c.SourceServiceType == sourceServiceType)
            .Select(c => new { c.UserId, c.DestinationServiceType })
            .ToListAsync(cancellationToken);

        if (mappings.Count == 0)
        {
            this.logger.LogInformation(
                "No destination mappings for source {Source}. Skipping {Count} users.",
                sourceServiceType,
                userIds.Count
            );
            return [];
        }

        List<string> destServiceTypes = mappings.Select(m => m.DestinationServiceType).Distinct().ToList();

        var connectedIntegrations = await this.dbContext.Integrations
            .Where(i => userIds.Contains(i.UserId) && destServiceTypes.Contains(i.ServiceType))
            .Select(i => new { i.UserId, i.ServiceType })
            .ToListAsync(cancellationToken);

        Dictionary<Guid, HashSet<string>> userMappings = mappings
            .GroupBy(m => m.UserId)
            .ToDictionary(g => g.Key, g => g.Select(m => m.DestinationServiceType).ToHashSet());

        HashSet<(Guid, string)> connected = connectedIntegrations
            .Select(i => (i.UserId, i.ServiceType))
            .ToHashSet();

        return userIds
            .Where(userId =>
                userMappings.TryGetValue(userId, out HashSet<string>? dests)
                && dests.Any(dest => connected.Contains((userId, dest))))
            .ToList();
    }
}
