namespace FitSync.Api.Features.OAuth.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class OAuthMetadataController(ILogger<OAuthMetadataController> logger) : ControllerBase
{
    private readonly ILogger<OAuthMetadataController> logger = logger;

    [HttpGet("~/.well-known/oauth-authorization-server")]
    [AllowAnonymous]
    public ActionResult GetMetadata()
    {
        this.logger.LogInformation("OAuth metadata requested.");

        string scheme =
            this.Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? this.Request.Scheme;
        string baseUrl = $"{scheme}://{this.Request.Host}";

        return this.Ok(
            new
            {
                issuer = baseUrl,
                authorization_endpoint = $"{baseUrl}/api/oauth/authorize",
                token_endpoint = $"{baseUrl}/api/oauth/token",
                registration_endpoint = $"{baseUrl}/connect/register",
                response_types_supported = new[] { "code" },
                grant_types_supported = new[] { "authorization_code" },
                token_endpoint_auth_methods_supported = new[] { "client_secret_post" },
                code_challenge_methods_supported = new[] { "S256" },
            }
        );
    }
}
