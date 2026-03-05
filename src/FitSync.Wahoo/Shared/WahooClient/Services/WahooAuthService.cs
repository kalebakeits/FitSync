namespace FitSync.Wahoo.Shared.WahooClient.Services;

using System.Net.Http.Json;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Wahoo.Shared.AuthData;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using FitSync.Wahoo.Shared.Configuration;
using FitSync.Wahoo.Shared.WahooClient.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class WahooAuthService(
    HttpClient httpClient,
    FitSyncDbContext dbContext,
    IOptions<WahooClientOptions> options,
    IEncryptionService encryptionService,
    ILogger<WahooAuthService> logger
) : IWahooAuthService
{
    private readonly HttpClient httpClient = httpClient;
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly IOptions<WahooClientOptions> options = options;
    private readonly IEncryptionService encryptionService = encryptionService;
    private readonly ILogger<WahooAuthService> logger = logger;
    private const int TokenExpiryBufferMinutes = 5;

    public async Task EnsureAuthenticatedAsync(
        Integration integration,
        CancellationToken cancellationToken = default
    )
    {
        WahooAuthData authData = integration.GetAuthData<WahooAuthData>(this.encryptionService);

        if (authData.TokenExpiresAt > DateTime.UtcNow.AddMinutes(TokenExpiryBufferMinutes))
        {
            return;
        }

        this.logger.LogInformation("Refreshing Wahoo token for user {UserId}.", integration.UserId);
        await this.RefreshTokenAsync(integration, authData, cancellationToken);
    }

    private async Task RefreshTokenAsync(
        Integration integration,
        WahooAuthData authData,
        CancellationToken cancellationToken
    )
    {
        string tokenUrl = $"{this.options.Value.BaseUrl.TrimEnd('/')}/oauth/token";

        FormUrlEncodedContent content = new(new Dictionary<string, string>
        {
            ["client_id"] = this.options.Value.ClientId,
            ["client_secret"] = this.options.Value.ClientSecret,
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = authData.RefreshToken,
        });

        HttpResponseMessage response = await this.httpClient.PostAsync(tokenUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        WahooTokenRefreshResponse token =
            await response.Content.ReadFromJsonAsync<WahooTokenRefreshResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Wahoo token refresh response could not be parsed.");

        authData.AccessToken = token.AccessToken;
        authData.RefreshToken = token.RefreshToken;
        authData.TokenExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
        integration.SetAuthData(authData, this.encryptionService);

        await this.dbContext.SaveChangesAsync(cancellationToken);
        this.logger.LogInformation("Wahoo token refreshed for user {UserId}.", integration.UserId);
    }
}
