namespace FitSync.ZwiftFetcher.Features.ZwiftClient;

using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.DTOs;
using FitSync.ZwiftFetcher.Features.ZwiftClient.Services;
using Microsoft.Extensions.Logging;

public class ZwiftClient(
    IZwiftAuthService authService,
    IZwiftApiService apiService,
    IZwiftActivityProcessor activityProcessor,
    ILogger<ZwiftClient> logger
) : IZwiftClient
{
    private readonly IZwiftAuthService authService = authService;
    private readonly IZwiftApiService apiService = apiService;
    private readonly IZwiftActivityProcessor activityProcessor = activityProcessor;
    private readonly ILogger<ZwiftClient> logger = logger;

    public async Task<List<FetchedActivity>> GetActivitiesAsync(
        ZwiftFetcherConfig config,
        int lookbackDays,
        CancellationToken cancellationToken
    )
    {
        await this.authService.EnsureAuthenticatedAsync(config, cancellationToken);

        var activities = await this.apiService.FetchActivitiesAsync(config, cancellationToken);

        var fetchedActivities = await this.activityProcessor.ProcessActivitiesAsync(
            activities,
            lookbackDays,
            cancellationToken
        );

        this.logger.LogInformation(
            "Processed {Count} activities for user {UserId}",
            fetchedActivities.Count,
            config.UserId
        );

        return fetchedActivities;
    }

    public Task AuthenticateAsync(
        ZwiftFetcherConfig config,
        string username,
        string password,
        CancellationToken cancellationToken
    )
    {
        return this.authService.AuthenticateAsync(config, username, password, cancellationToken);
    }
}
