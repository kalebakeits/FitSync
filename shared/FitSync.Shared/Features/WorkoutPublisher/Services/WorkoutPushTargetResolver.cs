namespace FitSync.Shared.Features.WorkoutPublisher.Services;

using FitSync.Database;
using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class WorkoutPushTargetResolver(
    FitSyncDbContext dbContext,
    IEnumerable<IWorkoutPublisherClient> clients,
    ILogger<WorkoutPushTargetResolver> logger
) : IWorkoutPushTargetResolver
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly Dictionary<string, IWorkoutPublisherClient> clientMap = clients.ToDictionary(
        c => c.ServiceType,
        c => c
    );
    private readonly ILogger<WorkoutPushTargetResolver> logger = logger;

    public async Task<WorkoutPushTarget> ResolveAsync(
        Guid userId,
        string serviceType,
        CancellationToken cancellationToken = default
    )
    {
        if (!this.clientMap.TryGetValue(serviceType, out IWorkoutPublisherClient? client))
        {
            this.logger.LogWarning(
                "Workout publishing is not supported for service type {ServiceType}.",
                serviceType
            );
            throw new NotSupportedException(
                $"Workout publishing is not supported for service type '{serviceType}'."
            );
        }

        Integration? integration = await this.dbContext.Integrations.FirstOrDefaultAsync(
            i => i.UserId == userId && i.ServiceType == serviceType,
            cancellationToken
        );

        if (integration is null)
        {
            this.logger.LogWarning(
                "No {ServiceType} integration found for user {UserId}.",
                serviceType,
                userId
            );
            throw new InvalidOperationException($"No {serviceType} integration found.");
        }

        return new WorkoutPushTarget(client, integration);
    }
}
