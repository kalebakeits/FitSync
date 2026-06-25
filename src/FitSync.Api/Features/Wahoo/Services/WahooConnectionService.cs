namespace FitSync.Api.Features.Wahoo.Services;

using FitSync.Api.Configurations;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using FitSync.Wahoo.Shared.AuthData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class WahooConnectionService(
    FitSyncDbContext context,
    IOptions<WahooOptions> options,
    IWahooOAuthService authService,
    IEncryptionService encryptionService,
    ILogger<WahooConnectionService> logger
) : IWahooConnectionService
{
    private readonly FitSyncDbContext context = context;
    private readonly IOptions<WahooOptions> options = options;
    private readonly IWahooOAuthService authService = authService;
    private readonly IEncryptionService encryptionService = encryptionService;
    private readonly ILogger<WahooConnectionService> logger = logger;

    public string BuildAuthorizeUrl(Guid userId)
    {
        string raw = $"{userId}|{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        string state = Uri.EscapeDataString(this.encryptionService.Encrypt(raw));
        string baseUrl = this.options.Value.BaseUrl.TrimEnd('/');
        string scopes = Uri.EscapeDataString(
            "email user_read user_write power_zones_read power_zones_write workouts_read workouts_write plans_read plans_write routes_read routes_write offline_data"
        );
        string redirectUri = Uri.EscapeDataString(this.options.Value.RedirectUri);
        string clientId = Uri.EscapeDataString(this.options.Value.ClientId);
        return $"{baseUrl}/oauth/authorize?client_id={clientId}&redirect_uri={redirectUri}&scope={scopes}&response_type=code&state={state}";
    }

    public async Task CompleteAuthorizationAsync(
        string state,
        string code,
        CancellationToken cancellationToken = default
    )
    {
        string decrypted = this.encryptionService.Decrypt(Uri.UnescapeDataString(state));
        string[] parts = decrypted.Split('|');
        if (
            parts.Length != 2
            || !Guid.TryParse(parts[0], out Guid userId)
            || !long.TryParse(parts[1], out long issuedAt)
        )
            throw new InvalidOperationException("Invalid OAuth state parameter.");

        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - issuedAt > 900)
            throw new InvalidOperationException("OAuth state parameter has expired.");

        WahooTokenResult token = await this.authService.ExchangeCodeAsync(code, cancellationToken);
        long wahooUserId = await this.authService.GetWahooUserIdAsync(
            token.AccessToken,
            cancellationToken
        );

        Integration? existing = await this.context.Integrations.FirstOrDefaultAsync(
            i => i.UserId == userId && i.ServiceType == ServiceTypes.Wahoo,
            cancellationToken
        );

        WahooAuthData authData =
            new()
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                TokenExpiresAt = token.ExpiresAtUtc,
                WahooUserId = wahooUserId,
            };

        if (existing == null)
        {
            Integration integration =
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ServiceType = ServiceTypes.Wahoo,
                    FailureCount = 0,
                    LookupValue = wahooUserId.ToString(),
                };
            integration.SetAuthData(authData, this.encryptionService);
            this.context.Integrations.Add(integration);
            await this.context.SaveChangesAsync(cancellationToken);

            this.context.FetcherConfigs.Add(
                new FetcherConfig
                {
                    Id = Guid.NewGuid(),
                    IntegrationId = integration.Id,
                    FetchIntervalMinutes = 360,
                }
            );
        }
        else
        {
            existing.SetAuthData(authData, this.encryptionService);
            existing.LookupValue = wahooUserId.ToString();
            existing.FailureCount = 0;
        }

        await this.context.SaveChangesAsync(cancellationToken);
        this.logger.LogInformation("Connected Wahoo account for user {UserId}.", userId);
    }
}
