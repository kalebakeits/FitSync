namespace FitSync.Garmin.Shared.GarminClient.Services;

using System.Text.RegularExpressions;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Garmin.Shared.AuthData;
using FitSync.Garmin.Shared.GarminClient.DTOs;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using Microsoft.Extensions.Logging;

public class GarminAuthService(
    IGarminApiClient apiClient,
    FitSyncDbContext dbContext,
    IEncryptionService encryptionService,
    ILogger<GarminAuthService> logger
) : IGarminAuthService
{
    private readonly IGarminApiClient apiClient = apiClient;
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly IEncryptionService encryptionService = encryptionService;
    private readonly ILogger<GarminAuthService> logger = logger;

    public async Task EnsureAuthenticatedAsync(Integration integration, CancellationToken ct)
    {
        GarminAuthData authData = integration.GetAuthData<GarminAuthData>(this.encryptionService);

        if (authData.HasValidOAuth2Token())
        {
            this.logger.LogDebug(
                "Garmin OAuth2 token still valid for user {UserId}, skipping auth.",
                integration.UserId
            );
            return;
        }

        if (authData.HasOAuth1Token())
        {
            this.logger.LogInformation(
                "OAuth2 token expired for user {UserId}, exchanging OAuth1.",
                integration.UserId
            );
            await this.ExchangeOAuth1ForOAuth2Async(integration, authData, ct);
            return;
        }

        this.logger.LogInformation(
            "No valid tokens for user {UserId}, running full sign-in.",
            integration.UserId
        );
        await this.FullSignInAsync(integration, authData, ct);
    }

    public async Task<bool> TryRefreshAsync(Integration integration, CancellationToken ct)
    {
        GarminAuthData authData = integration.GetAuthData<GarminAuthData>(this.encryptionService);

        if (!authData.HasOAuth1Token())
        {
            this.logger.LogWarning(
                "Cannot refresh — no OAuth1 token for user {UserId}.",
                integration.UserId
            );
            return false;
        }

        try
        {
            await this.ExchangeOAuth1ForOAuth2Async(integration, authData, ct);
            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "OAuth1 → OAuth2 exchange failed for user {UserId}.",
                integration.UserId
            );
            return false;
        }
    }

    private async Task ExchangeOAuth1ForOAuth2Async(
        Integration integration,
        GarminAuthData authData,
        CancellationToken ct
    )
    {
        ConsumerCredentials credentials = await this.apiClient.GetConsumerCredentialsAsync(ct);
        GarminOAuth2Token oauth2Token = await this.apiClient.GetOAuth2TokenAsync(
            authData.OAuth1Token!,
            authData.OAuth1TokenSecret!,
            credentials,
            ct
        );

        authData.OAuth2AccessToken = oauth2Token.AccessToken;
        authData.OAuth2ExpiresAt = DateTime.UtcNow.AddSeconds(oauth2Token.ExpiresIn);
        integration.SetAuthData(authData, this.encryptionService);
        await this.dbContext.SaveChangesAsync(ct);

        this.logger.LogInformation(
            "OAuth2 token refreshed for user {UserId}, expires at {ExpiresAt}.",
            integration.UserId,
            authData.OAuth2ExpiresAt
        );
    }

    private async Task FullSignInAsync(
        Integration integration,
        GarminAuthData authData,
        CancellationToken ct
    )
    {
        Flurl.Http.CookieJar jar = await this.apiClient.InitCookieJarAsync(ct);
        string csrfToken = await this.apiClient.GetCsrfTokenAsync(jar, ct);

        SendCredentialsResult credResult = await this.apiClient.SendCredentialsAsync(
            authData.Username,
            authData.Password,
            csrfToken,
            jar,
            ct
        );

        string ticket = ExtractTicket(credResult.RawResponseBody, integration.UserId);

        ConsumerCredentials credentials = await this.apiClient.GetConsumerCredentialsAsync(ct);
        (string oauth1Token, string oauth1Secret) = await this.apiClient.GetOAuth1TokenAsync(
            ticket,
            credentials,
            ct
        );
        GarminOAuth2Token oauth2Token = await this.apiClient.GetOAuth2TokenAsync(
            oauth1Token,
            oauth1Secret,
            credentials,
            ct
        );

        authData.OAuth1Token = oauth1Token;
        authData.OAuth1TokenSecret = oauth1Secret;
        authData.OAuth2AccessToken = oauth2Token.AccessToken;
        authData.OAuth2ExpiresAt = DateTime.UtcNow.AddSeconds(oauth2Token.ExpiresIn);

        integration.SetAuthData(authData, this.encryptionService);
        await this.dbContext.SaveChangesAsync(ct);

        this.logger.LogInformation(
            "Full Garmin sign-in complete for user {UserId}. OAuth2 expires at {ExpiresAt}.",
            integration.UserId,
            authData.OAuth2ExpiresAt
        );
    }

    private static string ExtractTicket(string rawBody, Guid userId)
    {
        Match match = new Regex(@"embed\?ticket=(?<ticket>[^""]+)""").Match(rawBody);

        if (!match.Success)
            throw new InvalidOperationException(
                $"Service ticket not found in Garmin SSO response for user {userId}. Response length: {rawBody.Length}"
            );

        string ticket = match.Groups["ticket"].Value;

        if (string.IsNullOrWhiteSpace(ticket))
            throw new InvalidOperationException(
                $"Extracted service ticket is empty for user {userId}."
            );

        return ticket;
    }
}
