namespace FitSync.Garmin.Shared.GarminClient.Services;

using System.Globalization;
using System.Net;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using FitSync.Garmin.Shared.Configuration;
using FitSync.Garmin.Shared.GarminClient.DTOs;
using FitSync.Shared.Features.RateLimiting;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class GarminApiService(
    IGarminHttpSender httpSender,
    IRateLimiter rateLimiter,
    IOptions<GarminFetcherOptions> options,
    ILogger<GarminApiService> logger
) : IGarminApiService
{
    private const string ActivityListPath = "/activitylist-service/activities/search/activities";
    private const string UserAgent = "GCM-iOS-5.7.2.1";
    private const string Origin = "https://sso.garmin.com";
    private const int ActivityPageSize = 100;
    // Bounds paging in case Garmin keeps answering with full pages.
    private const int MaxActivityPages = 10;

    private readonly IGarminHttpSender httpSender = httpSender;
    private readonly IRateLimiter rateLimiter = rateLimiter;
    private readonly IOptions<GarminFetcherOptions> options = options;
    private readonly ILogger<GarminApiService> logger = logger;

    public async Task<GarminActivityDto[]> FetchActivitiesAsync(
        Integration integration,
        int lookbackDays,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "Fetching Garmin activities for user {UserId} over {LookbackDays} days.",
            integration.UserId,
            lookbackDays
        );

        if (
            await this.rateLimiter.RateLimitedReachedAsync(
                ServiceType.GarminFetcher,
                this.options.Value.GarminApiRateLimits,
                cancellationToken
            )
        )
        {
            this.logger.LogWarning(
                "Garmin rate limit reached, skipping activity fetch for user {UserId}.",
                integration.UserId
            );
            return [];
        }

        string startDate = DateTime.UtcNow.AddDays(-lookbackDays)
            .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        List<GarminActivityDto> activities = [];

        for (int page = 0; page < MaxActivityPages; page++)
        {
            int start = page * ActivityPageSize;

            IFlurlResponse response = await this.httpSender.GetAsync(
                integration,
                accessToken =>
                    $"{this.options.Value.BaseUrl}{ActivityListPath}"
                        .WithOAuthBearerToken(accessToken)
                        .WithHeader("User-Agent", UserAgent)
                        .WithHeader("origin", Origin)
                        .WithHeader("NK", "NT")
                        .WithHeader("Accept", "application/json")
                        .SetQueryParams(
                            new
                            {
                                start,
                                limit = ActivityPageSize,
                                startDate,
                            }
                        ),
                cancellationToken
            );

            if (response.StatusCode == (int)HttpStatusCode.NoContent)
                break;

            if (response.StatusCode >= 400)
            {
                string errorBody = await response.GetStringAsync();
                throw new InvalidOperationException(
                    $"Garmin activity list failed ({response.StatusCode}): {errorBody}"
                );
            }

            GarminActivityDto[] pageActivities = await response.GetJsonAsync<GarminActivityDto[]>();
            activities.AddRange(pageActivities);

            if (pageActivities.Length < ActivityPageSize)
                break;
        }

        this.logger.LogInformation(
            "Garmin returned {Count} activities for user {UserId}.",
            activities.Count,
            integration.UserId
        );

        return [.. activities];
    }
}
