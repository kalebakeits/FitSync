namespace FitSync.ZwiftFetcher.Features.ZwiftClient.Services;

using FitSync.Database.Models;
using FitSync.ZwiftFetcher.Configuration;
using FitSync.ZwiftFetcher.Features.ZwiftClient.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

public class ZwiftApiService(
    HttpClient httpClient,
    IZwiftAuthService authService,
    ILogger<ZwiftApiService> logger,
    IOptions<ZwiftFetcherOptions> options
) : IZwiftApiService
{
    private readonly HttpClient httpClient = httpClient;
    private readonly IZwiftAuthService authService = authService;
    private readonly ILogger<ZwiftApiService> logger = logger;
    private readonly IOptions<ZwiftFetcherOptions> options = options;

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

        var response = await this.httpClient.GetAsync(
            Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(url, parameters),
            cancellationToken
        );

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(url, parameters),
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
        this.httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        this.httpClient.DefaultRequestHeaders.Accept.Clear();
        this.httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
        );
    }
}
