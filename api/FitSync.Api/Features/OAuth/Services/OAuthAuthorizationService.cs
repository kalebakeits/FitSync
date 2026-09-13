namespace FitSync.Api.Features.OAuth.Services;

using System.Security.Cryptography;
using System.Text;
using FitSync.Api.Features.OAuth.DTOs;
using FitSync.Api.Features.Tokens.DTOs;
using FitSync.Api.Features.Tokens.Services;
using FitSync.Database;
using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;

public class OAuthAuthorizationService(
    FitSyncDbContext db,
    IApiTokenService apiTokenService,
    ILogger<OAuthAuthorizationService> logger
) : IOAuthAuthorizationService
{
    private readonly FitSyncDbContext db = db;
    private readonly IApiTokenService apiTokenService = apiTokenService;
    private readonly ILogger<OAuthAuthorizationService> logger = logger;

    public async Task<OAuthConsentInfo> ValidateAuthorizeRequestAsync(
        string clientId,
        string redirectUri,
        string responseType,
        CancellationToken cancellationToken
    )
    {
        this.logger.LogInformation(
            "ValidateAuthorizeRequest called for clientId={ClientId} redirectUri={RedirectUri}",
            clientId,
            redirectUri
        );

        if (!string.Equals(responseType, "code", StringComparison.OrdinalIgnoreCase))
        {
            this.logger.LogWarning("Unsupported response_type: {ResponseType}", responseType);
            throw new InvalidOperationException(
                "Unsupported response_type. Only 'code' is supported."
            );
        }

        OAuthClient? client = await this.db.OAuthClients.FirstOrDefaultAsync(
            c => c.ClientId == clientId,
            cancellationToken
        );

        if (client is null)
        {
            this.logger.LogWarning("Unknown OAuth clientId={ClientId}", clientId);
            throw new UnauthorizedAccessException($"Unknown client_id: {clientId}");
        }

        if (!client.RedirectUris.Contains(redirectUri, StringComparer.OrdinalIgnoreCase))
        {
            this.logger.LogWarning(
                "redirect_uri {RedirectUri} not allowed for client {ClientId}",
                redirectUri,
                clientId
            );
            throw new UnauthorizedAccessException("redirect_uri is not allowed for this client.");
        }

        this.logger.LogInformation(
            "Authorize request valid for client {Name} ({ClientId})",
            client.Name,
            clientId
        );
        return new OAuthConsentInfo(client.Name, redirectUri, null);
    }

    public async Task<string> IssueCodeAsync(
        string clientId,
        Guid userId,
        string redirectUri,
        CancellationToken cancellationToken
    )
    {
        this.logger.LogInformation(
            "IssueCode called for clientId={ClientId} userId={UserId}",
            clientId,
            userId
        );

        OAuthClient? client = await this.db.OAuthClients.FirstOrDefaultAsync(
            c => c.ClientId == clientId,
            cancellationToken
        );

        if (client is null)
        {
            this.logger.LogWarning("IssueCode: unknown clientId={ClientId}", clientId);
            throw new UnauthorizedAccessException("Unknown client.");
        }

        string code = GenerateCode();

        OAuthCode oauthCode =
            new()
            {
                Id = Guid.NewGuid(),
                Code = code,
                ClientId = client.Id,
                UserId = userId,
                RedirectUri = redirectUri,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            };

        this.db.OAuthCodes.Add(oauthCode);
        await this.db.SaveChangesAsync(cancellationToken);

        this.logger.LogInformation(
            "Issued OAuth code for clientId={ClientId} userId={UserId}",
            clientId,
            userId
        );
        return code;
    }

    public async Task<string> ExchangeCodeAsync(
        string clientId,
        string clientSecret,
        string code,
        string redirectUri,
        CancellationToken cancellationToken
    )
    {
        this.logger.LogInformation("ExchangeCode called for clientId={ClientId}", clientId);

        string secretHash = HashSecret(clientSecret);

        OAuthClient? client = await this.db.OAuthClients.FirstOrDefaultAsync(
            c => c.ClientId == clientId && c.ClientSecretHash == secretHash,
            cancellationToken
        );

        if (client is null)
        {
            this.logger.LogWarning(
                "ExchangeCode: invalid credentials for clientId={ClientId}",
                clientId
            );
            throw new UnauthorizedAccessException("Invalid client credentials.");
        }

        OAuthCode? oauthCode = await this.db.OAuthCodes.FirstOrDefaultAsync(
            c => c.Code == code && c.ClientId == client.Id && c.UsedAt == null,
            cancellationToken
        );

        if (oauthCode is null)
        {
            this.logger.LogWarning(
                "ExchangeCode: code not found or used for clientId={ClientId}",
                clientId
            );
            throw new UnauthorizedAccessException("Invalid or expired authorization code.");
        }

        if (oauthCode.ExpiresAt < DateTime.UtcNow)
        {
            this.logger.LogWarning("ExchangeCode: code expired for clientId={ClientId}", clientId);
            throw new UnauthorizedAccessException("Authorization code has expired.");
        }

        if (!string.Equals(oauthCode.RedirectUri, redirectUri, StringComparison.OrdinalIgnoreCase))
        {
            this.logger.LogWarning(
                "ExchangeCode: redirect_uri mismatch for clientId={ClientId}",
                clientId
            );
            throw new UnauthorizedAccessException("redirect_uri mismatch.");
        }

        oauthCode.UsedAt = DateTime.UtcNow;
        await this.db.SaveChangesAsync(cancellationToken);

        CreateApiTokenResponse tokenResponse = await this.apiTokenService.CreateTokenAsync(
            oauthCode.UserId,
            $"OAuth: {client.Name}"
        );

        this.logger.LogInformation(
            "Exchanged code for API token for clientId={ClientId} userId={UserId}",
            clientId,
            oauthCode.UserId
        );
        return tokenResponse.Token;
    }

    private static string GenerateCode()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private static string HashSecret(string secret)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
