using System.Security.Claims;
using FitSync.Api.Services;

namespace FitSync.Api.Middleware;

public class SessionAuthenticationMiddleware(
    RequestDelegate next,
    ILogger<SessionAuthenticationMiddleware> logger
)
{
    private readonly RequestDelegate next = next;
    private readonly ILogger<SessionAuthenticationMiddleware> logger = logger;
    private const string SessionCookieName = "FitSync.Session";

    public async Task InvokeAsync(HttpContext context, ISessionService sessionService)
    {
        this.logger.LogDebug(
            "SessionAuthenticationMiddleware invoked for path: {Path}",
            context.Request.Path
        );

        if (context.Request.Cookies.TryGetValue(SessionCookieName, out string? sessionId))
        {
            this.logger.LogDebug("Session cookie found: {SessionId}", sessionId);

            Guid? userId = await sessionService.ValidateSessionAsync(sessionId);

            if (userId.HasValue)
            {
                this.logger.LogDebug(
                    "Session validated, setting user claims for userId: {UserId}",
                    userId.Value
                );

                Claim[] claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString())
                };

                ClaimsIdentity identity = new(claims, "Session");
                ClaimsPrincipal principal = new(identity);

                context.User = principal;

                this.logger.LogDebug("User authenticated via session: {UserId}", userId.Value);
            }
            else
            {
                this.logger.LogWarning("Invalid or expired session: {SessionId}", sessionId);
            }
        }
        else
        {
            this.logger.LogDebug("No session cookie found");
        }

        await this.next(context);
    }
}
