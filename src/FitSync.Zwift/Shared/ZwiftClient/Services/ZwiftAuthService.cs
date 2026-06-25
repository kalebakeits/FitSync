namespace FitSync.Zwift.Shared.ZwiftClient.Services;

using System.Net.Http.Json;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using FitSync.Zwift.Shared.AuthData;
using FitSync.Zwift.Shared.Configuration;
using FitSync.Zwift.Shared.ZwiftClient.DTOs;
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
        Integration integration,
        CancellationToken cancellationToken = default
    )
    {
        ZwiftAuthData authData = integration.GetAuthData<ZwiftAuthData>(this.encryptionService);

        if (
            !string.IsNullOrEmpty(authData.AccessToken) && !string.IsNullOrEmpty(authData.ProfileId)
        )
        {
            return;
        }

        await this.AuthenticateAsync(integration, authData, cancellationToken);
    }

    public async Task<bool> TryRefreshOrReauthenticateAsync(
        Integration integration,
        CancellationToken cancellationToken = default
    )
    {
        ZwiftAuthData authData = integration.GetAuthData<ZwiftAuthData>(this.encryptionService);

        if (!string.IsNullOrEmpty(authData.RefreshToken))
        {
            try
            {
                await this.RefreshAccessTokenAsync(integration, authData, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex, "Token refresh failed, attempting re-authentication.");
            }
        }

        try
        {
            await this.AuthenticateAsync(integration, authData, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Re-authentication failed for user {UserId}.",
                integration.UserId
            );
            return false;
        }
    }

    private async Task AuthenticateAsync(
        Integration integration,
        ZwiftAuthData authData,
        CancellationToken cancellationToken
    )
    {
        this.logger.LogInformation(
            "Authenticating with Zwift for user {UserId}.",
            integration.UserId
        );

        FormUrlEncodedContent content =
            new(
                new Dictionary<string, string>
                {
                    ["username"] = authData.Username,
                    ["password"] = authData.Password,
                    ["client_id"] = this.options.Value.ClientId,
                    ["grant_type"] = "password",
                }
            );

        HttpResponseMessage response = await this.httpClient.PostAsync(
            this.options.Value.AuthUrl,
            content,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            await this.dbContext.Integrations.Where(i => i.Id == integration.Id)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(i => i.FailureCount, i => i.FailureCount + 1),
                    cancellationToken
                );
            response.EnsureSuccessStatusCode();
        }

        AuthTokenResponse tokenResponse =
            await response.Content.ReadFromJsonAsync<AuthTokenResponse>(
                cancellationToken: cancellationToken
            ) ?? throw new Exception("Failed to deserialize Zwift auth response.");

        authData.AccessToken = tokenResponse.AccessToken;
        authData.RefreshToken = tokenResponse.RefreshToken;
        authData.ProfileId = await this.FetchProfileIdAsync(
            authData.AccessToken,
            cancellationToken
        );

        integration.SetAuthData(authData, this.encryptionService);

        await this.dbContext.Integrations.Where(i => i.Id == integration.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.FailureCount, 0), cancellationToken);

        await this.dbContext.SaveChangesAsync(cancellationToken);
        this.logger.LogInformation("Authenticated Zwift for user {UserId}.", integration.UserId);
    }

    private async Task RefreshAccessTokenAsync(
        Integration integration,
        ZwiftAuthData authData,
        CancellationToken cancellationToken
    )
    {
        this.logger.LogInformation("Refreshing Zwift token for user {UserId}.", integration.UserId);

        FormUrlEncodedContent content =
            new(
                new Dictionary<string, string>
                {
                    ["refresh_token"] = authData.RefreshToken!,
                    ["client_id"] = this.options.Value.ClientId,
                    ["grant_type"] = "refresh_token",
                }
            );

        HttpResponseMessage response = await this.httpClient.PostAsync(
            this.options.Value.AuthUrl,
            content,
            cancellationToken
        );
        response.EnsureSuccessStatusCode();

        AuthTokenResponse tokenResponse =
            await response.Content.ReadFromJsonAsync<AuthTokenResponse>(
                cancellationToken: cancellationToken
            ) ?? throw new Exception("Failed to deserialize Zwift token refresh response.");

        authData.AccessToken = tokenResponse.AccessToken;
        authData.RefreshToken = tokenResponse.RefreshToken;
        integration.SetAuthData(authData, this.encryptionService);
        await this.dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> FetchProfileIdAsync(
        string accessToken,
        CancellationToken cancellationToken
    )
    {
        string url = $"{this.options.Value.BaseUrl}/api/profiles/me";
        this.httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        this.httpClient.DefaultRequestHeaders.Accept.Clear();
        this.httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
        );
        HttpResponseMessage response = await this.httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        ZwiftProfileDto profile =
            await response.Content.ReadFromJsonAsync<ZwiftProfileDto>(
                cancellationToken: cancellationToken
            ) ?? throw new Exception("Failed to retrieve Zwift profile.");
        if (profile.Id == 0)
            throw new Exception("Invalid Zwift profile ID.");
        return profile.Id.ToString();
    }
}
