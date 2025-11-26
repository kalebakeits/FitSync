namespace FitSync.ZwiftFetcher.Features.ZwiftClient.Services;

using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using FitSync.ZwiftFetcher.Configuration;
using FitSync.ZwiftFetcher.Features.ZwiftClient.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class ZwiftAuthService(
    HttpClient httpClient,
    ILogger<ZwiftAuthService> logger,
    IOptions<ZwiftFetcherOptions> options,
    FitSyncDbContext dbContext,
    IEncryptionService encryptionService
) : IZwiftAuthService
{
    private readonly HttpClient httpClient = httpClient;
    private readonly ILogger<ZwiftAuthService> logger = logger;
    private readonly IOptions<ZwiftFetcherOptions> options = options;
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly IEncryptionService encryptionService = encryptionService;

    public async Task EnsureAuthenticatedAsync(
        ZwiftFetcherConfig config,
        CancellationToken cancellationToken = default
    )
    {
        if (!string.IsNullOrEmpty(config.AccessToken) && !string.IsNullOrEmpty(config.ProfileId))
        {
            this.logger.LogDebug("Already authenticated");
            return;
        }

        var zwiftCred = await this.dbContext.UserCredentials.FirstOrDefaultAsync(
            c => c.UserId == config.UserId && c.ServiceType == ServiceTypes.Zwift,
            cancellationToken
        );

        if (zwiftCred == null)
        {
            throw new InvalidOperationException(
                $"No Zwift credentials found for user {config.UserId}"
            );
        }

        (string username, string password) = zwiftCred.Decrypt(this.encryptionService);
        await this.AuthenticateAsync(config, username, password, cancellationToken);
    }

    public async Task AuthenticateAsync(
        ZwiftFetcherConfig config,
        string username,
        string password,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation("Authenticating with Zwift for user {UserId}...", config.UserId);

        var content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["username"] = username,
                ["password"] = password,
                ["client_id"] = this.options.Value.ClientId,
                ["grant_type"] = "password"
            }
        );

        var response = await this.httpClient.PostAsync(
            this.options.Value.AuthUrl,
            content,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            await this.dbContext.UserCredentials.Where(
                c => c.UserId == config.UserId && c.ServiceType == ServiceTypes.Zwift
            )
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(c => c.FailureCount, c => c.FailureCount + 1),
                    cancellationToken
                );

            this.logger.LogWarning(
                "Incremented credential failure count for user {UserId} due to Zwift auth failure",
                config.UserId
            );
        }

        response.EnsureSuccessStatusCode();

        var authData =
            await response.Content.ReadFromJsonAsync<AuthTokenResponse>(
                cancellationToken: cancellationToken
            ) ?? throw new Exception("Failed to deserialize authentication response.");

        config.AccessToken = authData.AccessToken;
        config.RefreshToken = authData.RefreshToken;

        await this.FetchAndSetProfileIdAsync(config, cancellationToken);

        // Reset credential failure count on success
        await this.dbContext.UserCredentials.Where(
            c => c.UserId == config.UserId && c.ServiceType == ServiceTypes.Zwift
        )
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(c => c.FailureCount, 0),
                cancellationToken
            );

        this.logger.LogInformation(
            "Reset credential failure count for user {UserId} after successful auth",
            config.UserId
        );
    }

    public async Task<bool> TryRefreshOrReauthenticateAsync(
        ZwiftFetcherConfig config,
        CancellationToken cancellationToken = default
    )
    {
        if (!string.IsNullOrEmpty(config.RefreshToken))
        {
            try
            {
                await this.RefreshAccessTokenAsync(config, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex, "Token refresh failed, will try re-authentication");
            }
        }

        try
        {
            var zwiftCred = await this.dbContext.UserCredentials.FirstOrDefaultAsync(
                c => c.UserId == config.UserId && c.ServiceType == ServiceTypes.Zwift,
                cancellationToken
            );

            if (zwiftCred == null)
            {
                this.logger.LogError("No Zwift credentials found for user {UserId}", config.UserId);
                return false;
            }

            (string username, string password) = zwiftCred.Decrypt(this.encryptionService);
            await this.AuthenticateAsync(config, username, password, cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Re-authentication with username/password failed");
            return false;
        }
    }

    private async Task RefreshAccessTokenAsync(
        ZwiftFetcherConfig config,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrEmpty(config.RefreshToken))
        {
            throw new Exception("Cannot refresh token: Refresh token is missing.");
        }

        this.logger.LogInformation("Refreshing access token for user {UserId}...", config.UserId);

        var content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["refresh_token"] = config.RefreshToken,
                ["client_id"] = this.options.Value.ClientId,
                ["grant_type"] = "refresh_token"
            }
        );

        var response = await this.httpClient.PostAsync(
            this.options.Value.AuthUrl,
            content,
            cancellationToken
        );

        response.EnsureSuccessStatusCode();

        var authData =
            await response.Content.ReadFromJsonAsync<AuthTokenResponse>(
                cancellationToken: cancellationToken
            ) ?? throw new Exception("Failed to deserialize token refresh response.");

        config.AccessToken = authData.AccessToken;
        config.RefreshToken = authData.RefreshToken;

        this.logger.LogInformation("Token refreshed successfully.");
    }

    private async Task FetchAndSetProfileIdAsync(
        ZwiftFetcherConfig config,
        CancellationToken cancellationToken
    )
    {
        this.logger.LogDebug("Fetching profile ID for user {UserId}...", config.UserId);

        string url = $"{this.options.Value.BaseUrl}/api/profiles/me";

        this.httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.AccessToken);
        this.httpClient.DefaultRequestHeaders.Accept.Clear();
        this.httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
        );

        var response = await this.httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var profileData = await response.Content.ReadFromJsonAsync<ZwiftProfileDto>(
            cancellationToken: cancellationToken
        );

        if (profileData is null || profileData.Id == 0)
        {
            throw new Exception("Failed to retrieve profile ID after authentication.");
        }

        config.ProfileId = profileData.Id.ToString();
        this.logger.LogInformation("Profile ID retrieved: {ProfileId}", config.ProfileId);
    }
}
