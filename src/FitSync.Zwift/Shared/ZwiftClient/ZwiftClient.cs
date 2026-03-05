namespace FitSync.Zwift.Shared.ZwiftClient;

using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.DTOs;
using FitSync.Zwift.Shared.ZwiftClient.Services;
using FitSync.Zwift.Shared.ZwiftClient;
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
        Integration integration,
        int lookbackDays,
        CancellationToken cancellationToken
    )
    {
        await this.authService.EnsureAuthenticatedAsync(integration, cancellationToken);

        var activities = await this.apiService.FetchActivitiesAsync(integration, cancellationToken);

        List<FetchedActivity> fetched = await this.activityProcessor.ProcessActivitiesAsync(
            activities,
            lookbackDays,
            cancellationToken
        );

        this.logger.LogInformation(
            "Processed {Count} Zwift activities for user {UserId}.",
            fetched.Count,
            integration.UserId
        );

        return fetched;
    }
}
