namespace FitSync.ZwiftFetcher.Features.ZwiftClient.Services;

using System.Net;
using System.Net.Http.Headers;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using FitSync.Shared.Features.RateLimiting;
using FitSync.ZwiftFetcher.Configuration;
using FitSync.ZwiftFetcher.Features.ZwiftClient.DTOs;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

public class ZwiftApiService(
    HttpClient httpClient,
    IZwiftAuthService authService,
    ILogger<ZwiftApiService> logger,
    IOptions<ZwiftFetcherOptions> options,
    IRateLimiter rateLimiter
) : IZwiftApiService
{
    private readonly HttpClient httpClient = httpClient;
    private readonly IZwiftAuthService authService = authService;
    private readonly ILogger<ZwiftApiService> logger = logger;
    private readonly IOptions<ZwiftFetcherOptions> options = options;
    private readonly IRateLimiter rateLimiter = rateLimiter;

    public async Task<ZwiftActivityDto[]> FetchActivitiesAsync(
        ZwiftFetcherConfig config,
        CancellationToken cancellationToken = default
    )
    {
        string url = $"{this.options.Value.BaseUrl}/api/profiles/{config.ProfileId}/activities";

        this.logger.LogInformation(
            "Fetching activities from Zwift for profile {ProfileId}, URL: {Url}",
            config.ProfileId,
            url
        );

        this.SetAuthHeaders(config.AccessToken);

        var parameters = new Dictionary<string, string?> { ["start"] = "0", ["limit"] = "50" };

        ServiceType type = ServiceType.ZwiftFetcher;
        int limit = this.options.Value.ZwfitApiRateLimit;
        if (await this.rateLimiter.RateLimitedReachedAsync(type, limit, cancellationToken))
            return [];

        var response = await this.httpClient.GetAsync(
            QueryHelpers.AddQueryString(url, parameters),
            cancellationToken
        );

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            this.logger.LogWarning("Received 401, attempting to refresh token...");
            bool refreshed = await this.authService.TryRefreshOrReauthenticateAsync(
                config,
                cancellationToken
            );

            if (!refreshed)
            {
                throw new Exception("Failed to refresh authentication after 401");
            }

            this.SetAuthHeaders(config.AccessToken);
            response = await this.httpClient.GetAsync(
                QueryHelpers.AddQueryString(url, parameters),
                cancellationToken
            );
        }

        response.EnsureSuccessStatusCode();

        string jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        ZwiftActivityDto[] activities =
            JToken.Parse(jsonResponse).ToObject<ZwiftActivityDto[]>() ?? [];

        this.logger.LogInformation(
            "Zwift API response contains: {NumActivities} for User {UserId}",
            activities.Length,
            config.UserId
        );

        return activities;
    }

    private void SetAuthHeaders(string? accessToken)
    {
        this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            accessToken
        );
        this.httpClient.DefaultRequestHeaders.Accept.Clear();
        this.httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
    }
}
