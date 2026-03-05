using System.Security.Cryptography;
using FitSync.Database;
using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FitSync.Api.Services;

public interface ISessionService
{
    Task<string> CreateSessionAsync(Guid userId);
    Task<Guid?> ValidateSessionAsync(string sessionId);
    Task InvalidateSessionAsync(string sessionId);
    Task InvalidateAllUserSessionsAsync(Guid userId);
}

public class SessionService(FitSyncDbContext context, ILogger<SessionService> logger)
    : ISessionService
{
    private readonly FitSyncDbContext context = context;
    private readonly ILogger<SessionService> logger = logger;
    private const int SessionExpirationDays = 30;

    public async Task<string> CreateSessionAsync(Guid userId)
    {
        this.logger.LogInformation("Creating session for user: {UserId}", userId);

        string sessionId = GenerateSecureSessionId();

        Session session =
            new()
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(SessionExpirationDays)
            };

        this.context.Sessions.Add(session);
        await this.context.SaveChangesAsync();

        this.logger.LogInformation(
            "Session created successfully: {SessionId} for user: {UserId}",
            sessionId,
            userId
        );

        return sessionId;
    }

    public async Task<Guid?> ValidateSessionAsync(string sessionId)
    {
        this.logger.LogDebug("Validating session: {SessionId}", sessionId);

        Session? session = await this.context.Sessions.Include(s => s.User)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session == null)
        {
            this.logger.LogWarning("Session not found: {SessionId}", sessionId);
            return null;
        }

        if (session.ExpiresAt < DateTime.UtcNow)
        {
            this.logger.LogWarning("Session expired: {SessionId}", sessionId);
            await InvalidateSessionAsync(sessionId);
            return null;
        }

        this.logger.LogDebug(
            "Session validated successfully: {SessionId} for user: {UserId}",
            sessionId,
            session.UserId
        );

        return session.UserId;
    }

    public async Task InvalidateSessionAsync(string sessionId)
    {
        this.logger.LogInformation("Invalidating session: {SessionId}", sessionId);

        Session? session = await this.context.Sessions.FirstOrDefaultAsync(
            s => s.SessionId == sessionId
        );

        if (session != null)
        {
            this.context.Sessions.Remove(session);
            await this.context.SaveChangesAsync();

            this.logger.LogInformation("Session invalidated successfully: {SessionId}", sessionId);
        }
    }

    public async Task InvalidateAllUserSessionsAsync(Guid userId)
    {
        this.logger.LogInformation("Invalidating all sessions for user: {UserId}", userId);

        int count = await this.context.Sessions
            .Where(s => s.UserId == userId)
            .ExecuteDeleteAsync();

        this.logger.LogInformation(
            "Invalidated {Count} sessions for user: {UserId}",
            count,
            userId
        );
    }

    private static string GenerateSecureSessionId()
    {
        byte[] randomBytes = new byte[32];
        RandomNumberGenerator.Fill(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
