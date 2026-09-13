namespace FitSync.Api.Features.OAuth.Controllers;

using FitSync.Api.Features.OAuth.DTOs;
using FitSync.Api.Features.OAuth.Services;
using FitSync.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("oauth")]
[ApiExplorerSettings(IgnoreApi = true)]
public class OAuthController(
    IOAuthAuthorizationService oauthService,
    ICurrentUserService currentUserService,
    ILogger<OAuthController> logger
) : ControllerBase
{
    private readonly IOAuthAuthorizationService oauthService = oauthService;
    private readonly ICurrentUserService currentUserService = currentUserService;
    private readonly ILogger<OAuthController> logger = logger;

    [HttpGet("authorize")]
    [AllowAnonymous]
    public async Task<ActionResult> Authorize(
        [FromQuery(Name = "client_id")] string clientId,
        [FromQuery(Name = "redirect_uri")] string redirectUri,
        [FromQuery(Name = "response_type")] string responseType,
        [FromQuery(Name = "state")] string? state,
        CancellationToken cancellationToken
    )
    {
        this.logger.LogInformation(
            "Authorize called for clientId={ClientId} redirectUri={RedirectUri}",
            clientId,
            redirectUri
        );

        if (!this.User.Identity?.IsAuthenticated ?? true)
        {
            string returnUrl = this.Request.Path + this.Request.QueryString;
            this.logger.LogInformation(
                "User not authenticated, redirecting to login with returnUrl={ReturnUrl}",
                returnUrl
            );
            return this.Redirect($"/login?next={Uri.EscapeDataString(returnUrl)}");
        }

        OAuthConsentInfo consentInfo = await this.oauthService.ValidateAuthorizeRequestAsync(
            clientId,
            redirectUri,
            responseType,
            cancellationToken
        );

        string consentUrl =
            $"/oauth/consent?client_id={Uri.EscapeDataString(clientId)}"
            + $"&client_name={Uri.EscapeDataString(consentInfo.ClientName)}"
            + $"&redirect_uri={Uri.EscapeDataString(redirectUri)}"
            + $"&response_type={Uri.EscapeDataString(responseType)}"
            + (state is not null ? $"&state={Uri.EscapeDataString(state)}" : "");

        this.logger.LogInformation(
            "Authorize validated, redirecting to consent for client {Name}",
            consentInfo.ClientName
        );
        return this.Redirect(consentUrl);
    }

    [HttpPost("approve")]
    [Authorize]
    public async Task<ActionResult> Approve(
        [FromForm(Name = "client_id")] string clientId,
        [FromForm(Name = "redirect_uri")] string redirectUri,
        [FromForm(Name = "state")] string? state,
        CancellationToken cancellationToken
    )
    {
        Guid userId = this.currentUserService.GetUserId();
        this.logger.LogInformation(
            "Approve called for clientId={ClientId} userId={UserId}",
            clientId,
            userId
        );

        string code = await this.oauthService.IssueCodeAsync(
            clientId,
            userId,
            redirectUri,
            cancellationToken
        );

        string callback =
            $"{redirectUri}?code={Uri.EscapeDataString(code)}"
            + (state is not null ? $"&state={Uri.EscapeDataString(state)}" : "");

        this.logger.LogInformation(
            "Approved OAuth for clientId={ClientId} userId={UserId}, redirecting with code.",
            clientId,
            userId
        );
        return this.Redirect(callback);
    }

    [HttpPost("deny")]
    [AllowAnonymous]
    public ActionResult Deny(
        [FromForm(Name = "redirect_uri")] string redirectUri,
        [FromForm(Name = "state")] string? state
    )
    {
        this.logger.LogInformation("OAuth denied, redirecting to {RedirectUri}", redirectUri);
        string callback =
            $"{redirectUri}?error=access_denied"
            + (state is not null ? $"&state={Uri.EscapeDataString(state)}" : "");
        return this.Redirect(callback);
    }

    [HttpPost("token")]
    [AllowAnonymous]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<ActionResult<OAuthTokenResponse>> Token(
        [FromForm(Name = "grant_type")] string grantType,
        [FromForm(Name = "code")] string code,
        [FromForm(Name = "redirect_uri")] string redirectUri,
        [FromForm(Name = "client_id")] string clientId,
        [FromForm(Name = "client_secret")] string clientSecret,
        CancellationToken cancellationToken
    )
    {
        this.logger.LogInformation(
            "Token exchange called for clientId={ClientId} grantType={GrantType}",
            clientId,
            grantType
        );

        if (!string.Equals(grantType, "authorization_code", StringComparison.OrdinalIgnoreCase))
        {
            this.logger.LogWarning("Unsupported grant_type: {GrantType}", grantType);
            return this.BadRequest(new { error = "unsupported_grant_type" });
        }

        string accessToken = await this.oauthService.ExchangeCodeAsync(
            clientId,
            clientSecret,
            code,
            redirectUri,
            cancellationToken
        );

        this.logger.LogInformation("Token issued for clientId={ClientId}", clientId);
        return this.Ok(new OAuthTokenResponse(accessToken, "bearer"));
    }
}
