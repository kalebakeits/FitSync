namespace FitSync.Shared.Features.Fetcher.Services;

using FitSync.Database.Models;
using FitSync.Shared.Configuration;
using FitSync.Shared.Features.Fetcher.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class FetchOrchestrator(
    IServiceProvider serviceProvider,
    ILogger<FetchOrchestrator> logger,
    IOptions<FetcherOptions> options
) : IFetchOrchestrator
{
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly ILogger<FetchOrchestrator> logger = logger;
    private readonly IOptions<FetcherOptions> options = options;

    public async Task ProcessUsersAsync(User[] users, CancellationToken cancellationToken = default)
    {
        await Parallel.ForEachAsync(
            users,
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = this.options.Value.MaxParallelUsers
            },
            async (user, ct) => await this.ProcessUserAsync(user, ct)
        );
    }

    private async Task ProcessUserAsync(User user, CancellationToken cancellationToken)
    {
        using IServiceScope scope = this.serviceProvider.CreateScope();
        IFetcherService fetcherService =
            scope.ServiceProvider.GetRequiredService<IFetcherService>();

        List<FetchedActivity> activities = await fetcherService.GetActivitiesAsync(
            user,
            cancellationToken
        );

        this.logger.LogInformation(
            "Fetched {Count} activities for user {UserId}",
            activities.Count,
            user.Id
        );

        await this.ProcessActivitiesAsync(user.Id, activities, cancellationToken);
    }

    private async Task ProcessActivitiesAsync(
        Guid userId,
        List<FetchedActivity> activities,
        CancellationToken cancellationToken
    )
    {
        await Parallel.ForEachAsync(
            activities,
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = this.options.Value.MaxParallelActivities
            },
            async (activity, ct) =>
            {
                using IServiceScope scope = this.serviceProvider.CreateScope();
                IActivityPersistenceService persistenceService =
                    scope.ServiceProvider.GetRequiredService<IActivityPersistenceService>();
                await persistenceService.SaveAndPublishAsync(userId, activity, ct);
            }
        );
    }
}
