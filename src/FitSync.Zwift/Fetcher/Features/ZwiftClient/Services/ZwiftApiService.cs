namespace FitSync.Zwift.Fetcher.Features.ZwiftClient.Services;

using System.Net;
using System.Net.Http.Headers;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using FitSync.Zwift.Shared.AuthData;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using FitSync.Shared.Features.RateLimiting;
using FitSync.Zwift.Fetcher.Configuration;
using FitSync.Zwift.Fetcher.Features.ZwiftClient.DTOs;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

public class ZwiftApiService(
    HttpClient httpClient,
    IZwiftAuthService authService,
    ILogger<ZwiftApiService> logger,
    IOptions<ZwiftFetcherOptions> options,
    IRateLimiter rateLimiter,
    IEncryptionService encryptionService
) : IZwiftApiService
{
    private readonly HttpClient httpClient = httpClient;
    private readonly IZwiftAuthService authService = authService;
    private readonly ILogger<ZwiftApiService> logger = logger;
    private readonly IOptions<ZwiftFetcherOptions> options = options;
    private readonly IRateLimiter rateLimiter = rateLimiter;
    private readonly IEncryptionService encryptionService = encryptionService;

    public async Task<ZwiftActivityDto[]> FetchActivitiesAsync(
        Integration integration,
        CancellationToken cancellationToken = default
    )
    {
        ZwiftAuthData authData = integration.GetAuthData<ZwiftAuthData>(this.encryptionService);
        string url = $"{this.options.Value.BaseUrl}/api/profiles/{authData.ProfileId}/activities";

        this.logger.LogInformation("Fetching Zwift activities for profile {ProfileId}.", authData.ProfileId);
        this.SetAuthHeaders(authData.AccessToken);

        Dictionary<string, string?> parameters = new() { ["start"] = "0", ["limit"] = "50" };

        if (await this.rateLimiter.RateLimitedReachedAsync(ServiceType.ZwiftFetcher, this.options.Value.ZwfitApiRateLimit, cancellationToken))
            return [];

        HttpResponseMessage response = await this.httpClient.GetAsync(
            QueryHelpers.AddQueryString(url, parameters), cancellationToken
        );

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            this.logger.LogWarning("Received 401 from Zwift, attempting token refresh.");
            bool refreshed = await this.authService.TryRefreshOrReauthenticateAsync(integration, cancellationToken);
            if (!refreshed) throw new Exception("Failed to refresh Zwift authentication after 401.");
            authData = integration.GetAuthData<ZwiftAuthData>(this.encryptionService);
            this.SetAuthHeaders(authData.AccessToken);
            response = await this.httpClient.GetAsync(QueryHelpers.AddQueryString(url, parameters), cancellationToken);
        }

        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync(cancellationToken);
        ZwiftActivityDto[] activities = JToken.Parse(json).ToObject<ZwiftActivityDto[]>() ?? [];

        this.logger.LogInformation("Zwift returned {Count} activities for user {UserId}.", activities.Length, integration.UserId);
        return activities;
    }

    private void SetAuthHeaders(string? accessToken)
    {
        this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        this.httpClient.DefaultRequestHeaders.Accept.Clear();
        this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
}
