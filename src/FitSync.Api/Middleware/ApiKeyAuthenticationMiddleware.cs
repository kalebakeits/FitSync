namespace FitSync.Api.Middleware;

using System.Security.Claims;
using FitSync.Api.Features.Tokens.Services;

public class ApiKeyAuthenticationMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate next = next;

    public async Task InvokeAsync(HttpContext context, IApiTokenService apiTokenService)
    {
        if (!context.Request.Path.StartsWithSegments("/mcp"))
        {
            await this.next(context);
            return;
        }

        string? authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (
            authHeader is null
            || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
        )
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        string rawToken = authHeader["Bearer ".Length..].Trim();
        Guid? userId = await apiTokenService.ValidateTokenAsync(rawToken);

        if (userId is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        ClaimsIdentity identity =
            new([new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString())], "ApiKey");
        context.User = new ClaimsPrincipal(identity);

        await this.next(context);
    }
}
