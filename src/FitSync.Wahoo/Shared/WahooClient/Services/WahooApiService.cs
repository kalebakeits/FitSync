namespace FitSync.Wahoo.Shared.WahooClient.Services;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FitSync.Database.Models;
using FitSync.Wahoo.Shared.AuthData;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using FitSync.Wahoo.Shared.Configuration;
using FitSync.Wahoo.Shared.WahooClient.DTOs;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class WahooApiService(
    HttpClient httpClient,
    IWahooAuthService authService,
    IOptions<WahooClientOptions> options,
    IEncryptionService encryptionService,
    ILogger<WahooApiService> logger
) : IWahooApiService
{
    private readonly HttpClient httpClient = httpClient;
    private readonly IWahooAuthService authService = authService;
    private readonly IOptions<WahooClientOptions> options = options;
    private readonly IEncryptionService encryptionService = encryptionService;
    private readonly ILogger<WahooApiService> logger = logger;

    public async Task<List<WahooWorkoutDto>> FetchWorkoutsAsync(
        Integration integration,
        int lookbackDays,
        CancellationToken cancellationToken = default
    )
    {
        await this.authService.EnsureAuthenticatedAsync(integration, cancellationToken);

        string url = $"{this.options.Value.BaseUrl.TrimEnd('/')}/v1/workouts";
        List<WahooWorkoutDto> allWorkouts = [];
        int page = 1;
        DateTime cutoff = DateTime.UtcNow.AddDays(-lookbackDays);

        while (true)
        {
            WahooAuthData authData = integration.GetAuthData<WahooAuthData>(this.encryptionService);
            Dictionary<string, string?> parameters = new()
            {
                ["page"] = page.ToString(),
                ["per_page"] = "30",
                ["order"] = "descending",
                ["sort"] = "starts",
            };

            HttpRequestMessage request = new(HttpMethod.Get, QueryHelpers.AddQueryString(url, parameters));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authData.AccessToken);

            HttpResponseMessage response = await this.httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                this.logger.LogWarning("Received 401 from Wahoo, retrying after token refresh.");
                await this.authService.EnsureAuthenticatedAsync(integration, cancellationToken);
                authData = integration.GetAuthData<WahooAuthData>(this.encryptionService);
                request = new HttpRequestMessage(HttpMethod.Get, QueryHelpers.AddQueryString(url, parameters));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authData.AccessToken);
                response = await this.httpClient.SendAsync(request, cancellationToken);
            }

            response.EnsureSuccessStatusCode();

            WahooWorkoutsResponse pageResult =
                await response.Content.ReadFromJsonAsync<WahooWorkoutsResponse>(cancellationToken)
                ?? new WahooWorkoutsResponse();

            List<WahooWorkoutDto> inWindow = pageResult.Workouts
                .Where(w => w.Starts >= cutoff && w.WorkoutSummary?.File?.Url != null)
                .ToList();

            allWorkouts.AddRange(inWindow);

            bool hasMore = pageResult.Workouts.Count == 30 && pageResult.Workouts.Last().Starts >= cutoff;
            if (!hasMore) break;
            page++;
        }

        this.logger.LogInformation(
            "Fetched {Count} workouts for user {UserId} within last {Days} days.",
            allWorkouts.Count, integration.UserId, lookbackDays
        );

        return allWorkouts;
    }
}
