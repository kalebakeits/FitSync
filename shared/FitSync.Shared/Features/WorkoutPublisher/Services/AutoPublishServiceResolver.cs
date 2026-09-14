namespace FitSync.Shared.Features.WorkoutPublisher.Services;

using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.ActivityIngest.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class AutoPublishServiceResolver(
    FitSyncDbContext dbContext,
    ISportCategoryResolver sportCategoryResolver,
    IEnumerable<IWorkoutPublisherClient> clients,
    ILogger<AutoPublishServiceResolver> logger
) : IAutoPublishServiceResolver
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly ISportCategoryResolver sportCategoryResolver = sportCategoryResolver;
    private readonly Dictionary<string, IWorkoutPublisherClient> clientMap = clients.ToDictionary(
        c => c.ServiceType,
        c => c
    );
    private readonly ILogger<AutoPublishServiceResolver> logger = logger;

    public async Task<List<string>> ResolveServiceTypesAsync(
        Guid userId,
        Guid workoutId,
        CancellationToken cancellationToken = default
    )
    {
        Workout? workout = await this.dbContext.Workouts.FirstOrDefaultAsync(
            w => w.Id == workoutId && w.UserId == userId,
            cancellationToken
        );

        if (workout is null)
        {
            this.logger.LogWarning(
                "Auto-publish skipped: workout {WorkoutId} not found for user {UserId}.",
                workoutId,
                userId
            );
            return [];
        }

        string categoryName = this.sportCategoryResolver.ToCategoryName(
            this.sportCategoryResolver.Resolve(workout.Sport)
        );

        List<string> matchedServiceTypes = await this.dbContext.AutoPublishSettings.Where(
            s => s.UserId == userId && s.SportCategory == categoryName
        )
            .Select(s => s.ServiceType)
            .Distinct()
            .ToListAsync(cancellationToken);

        this.logger.LogInformation(
            "Auto-publish lookup for user {UserId}, workout {WorkoutId}, category {Category}: {Count} configured service(s) [{ServiceTypes}].",
            userId,
            workoutId,
            categoryName,
            matchedServiceTypes.Count,
            string.Join(", ", matchedServiceTypes)
        );

        if (matchedServiceTypes.Count == 0)
        {
            this.logger.LogInformation(
                "Auto-publish skipped for user {UserId}: no {Category} setting configured, scheduling calendar-only.",
                userId,
                categoryName
            );
            return [];
        }

        HashSet<string> connectedServiceTypes = (
            await this.dbContext.Integrations.Where(i =>
                    i.UserId == userId && matchedServiceTypes.Contains(i.ServiceType)
                )
                .Select(i => i.ServiceType)
                .Distinct()
                .ToListAsync(cancellationToken)
        ).ToHashSet();

        List<string> serviceTypes = [];

        foreach (string serviceType in matchedServiceTypes)
        {
            if (!this.clientMap.ContainsKey(serviceType))
            {
                this.logger.LogWarning(
                    "Auto-publish skipping {ServiceType} for user {UserId}: it does not support workout publishing, publishing to the remaining services only.",
                    serviceType,
                    userId
                );
                continue;
            }

            if (!connectedServiceTypes.Contains(serviceType))
            {
                this.logger.LogWarning(
                    "Auto-publish skipping {ServiceType} for user {UserId}: it is not connected, publishing to the remaining services only.",
                    serviceType,
                    userId
                );
                continue;
            }

            serviceTypes.Add(serviceType);
        }

        if (serviceTypes.Count == 0)
        {
            this.logger.LogWarning(
                "Auto-publish skipped for user {UserId}, workout {WorkoutId}: none of the {Count} {Category} service(s) [{ServiceTypes}] are connected or publishable, scheduling calendar-only.",
                userId,
                workoutId,
                matchedServiceTypes.Count,
                categoryName,
                string.Join(", ", matchedServiceTypes)
            );
            return [];
        }

        this.logger.LogInformation(
            "Auto-publish resolved {Count} service(s) [{ServiceTypes}] for user {UserId}, workout {WorkoutId}, category {Category}.",
            serviceTypes.Count,
            string.Join(", ", serviceTypes),
            userId,
            workoutId,
            categoryName
        );
        return serviceTypes;
    }
}
