using System.Security.Claims;

namespace FitSync.Api.Services;

public interface ICurrentUserService
{
    Guid GetUserId();
}

public class CurrentUserService(
    IHttpContextAccessor httpContextAccessor,
    ILogger<CurrentUserService> logger
) : ICurrentUserService
{
    private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
    private readonly ILogger<CurrentUserService> logger = logger;

    public Guid GetUserId()
    {
        ClaimsPrincipal? user = this.httpContextAccessor.HttpContext?.User;

        if (this.logger.IsEnabled(LogLevel.Debug))
        {
            this.logger.LogDebug(
                "Getting user ID from claims. User authenticated: {IsAuthenticated}",
                user?.Identity?.IsAuthenticated
            );
            this.logger.LogDebug(
                "User claims: {@Claims}",
                user?.Claims.Select(c => new { c.Type, c.Value })
            );
        }

        Claim? userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
        {
            this.logger.LogError("NameIdentifier claim not found in token");
            throw new UnauthorizedAccessException("User ID claim not found");
        }

        Guid userId = Guid.Parse(userIdClaim.Value);
        this.logger.LogDebug("Retrieved user ID: {UserId}", userId);

        return userId;
    }
}
