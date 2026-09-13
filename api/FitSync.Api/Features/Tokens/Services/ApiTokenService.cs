namespace FitSync.Api.Features.Tokens.Services;

using System.Security.Cryptography;
using System.Text;
using FitSync.Api.Features.Tokens.DTOs;
using FitSync.Database;
using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class ApiTokenService(FitSyncDbContext db, ILogger<ApiTokenService> logger)
    : IApiTokenService
{
    private readonly FitSyncDbContext db = db;
    private readonly ILogger<ApiTokenService> logger = logger;

    public async Task<List<ApiTokenResponse>> GetTokensAsync(Guid userId)
    {
        this.logger.LogInformation("GetTokens called for user {UserId}.", userId);
        return await this.db.ApiTokens.Where(t => t.UserId == userId && t.RevokedAt == null)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new ApiTokenResponse(t.Id, t.Name, t.CreatedAt, t.LastUsedAt))
            .ToListAsync();
    }

    public async Task<CreateApiTokenResponse> CreateTokenAsync(Guid userId, string name)
    {
        this.logger.LogInformation(
            "CreateToken called for user {UserId}, name {Name}.",
            userId,
            name
        );

        string rawToken = GenerateToken();
        string tokenHash = HashToken(rawToken);

        ApiToken token =
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = name,
                TokenHash = tokenHash,
            };

        this.db.ApiTokens.Add(token);
        await this.db.SaveChangesAsync();

        this.logger.LogInformation(
            "Created API token {TokenId} for user {UserId}.",
            token.Id,
            userId
        );
        return new CreateApiTokenResponse(token.Id, token.Name, rawToken, token.CreatedAt);
    }

    public async Task RevokeTokenAsync(Guid userId, Guid tokenId)
    {
        this.logger.LogInformation(
            "RevokeToken called for token {TokenId}, user {UserId}.",
            tokenId,
            userId
        );

        ApiToken? token = await this.db.ApiTokens.FirstOrDefaultAsync(
            t => t.Id == tokenId && t.UserId == userId && t.RevokedAt == null
        );

        if (token is null)
        {
            this.logger.LogWarning("Token {TokenId} not found for user {UserId}.", tokenId, userId);
            return;
        }

        token.RevokedAt = DateTime.UtcNow;
        await this.db.SaveChangesAsync();
        this.logger.LogInformation("Revoked token {TokenId}.", tokenId);
    }

    public async Task<Guid?> ValidateTokenAsync(string rawToken)
    {
        string tokenHash = HashToken(rawToken);

        ApiToken? token = await this.db.ApiTokens.FirstOrDefaultAsync(
            t => t.TokenHash == tokenHash && t.RevokedAt == null
        );

        if (token is null)
            return null;

        token.LastUsedAt = DateTime.UtcNow;
        await this.db.SaveChangesAsync();
        return token.UserId;
    }

    private static string GenerateToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);
        return "fsk_"
            + Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private static string HashToken(string rawToken)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
