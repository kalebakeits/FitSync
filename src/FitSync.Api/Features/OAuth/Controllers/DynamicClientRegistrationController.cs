namespace FitSync.Api.Features.OAuth.Controllers;

using System.Security.Cryptography;
using System.Text;
using FitSync.Api.Features.OAuth.DTOs;
using FitSync.Database;
using FitSync.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class DynamicClientRegistrationController(
    FitSyncDbContext db,
    ILogger<DynamicClientRegistrationController> logger
) : ControllerBase
{
    private readonly FitSyncDbContext db = db;
    private readonly ILogger<DynamicClientRegistrationController> logger = logger;

    [HttpPost("~/connect/register")]
    [AllowAnonymous]
    public async Task<ActionResult<DynamicClientRegistrationResponse>> Register(
        [FromBody] DynamicClientRegistrationRequest request,
        CancellationToken cancellationToken
    )
    {
        this.logger.LogInformation(
            "Dynamic client registration request for client_name={ClientName}",
            request.ClientName
        );

        if (string.IsNullOrWhiteSpace(request.ClientName))
        {
            return this.BadRequest(
                new
                {
                    error = "invalid_client_metadata",
                    error_description = "client_name is required."
                }
            );
        }

        if (request.RedirectUris is null || request.RedirectUris.Length == 0)
        {
            return this.BadRequest(
                new
                {
                    error = "invalid_client_metadata",
                    error_description = "redirect_uris is required."
                }
            );
        }

        string clientId = Convert
            .ToHexString(RandomNumberGenerator.GetBytes(16))
            .ToLowerInvariant();
        string clientSecret = Convert
            .ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        OAuthClient client =
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                ClientSecretHash = HashSecret(clientSecret),
                Name = request.ClientName,
                RedirectUris = request.RedirectUris,
            };

        this.db.OAuthClients.Add(client);
        await this.db.SaveChangesAsync(cancellationToken);

        this.logger.LogInformation(
            "Dynamic client registration complete: client_id={ClientId} client_name={ClientName}",
            clientId,
            request.ClientName
        );

        return this.StatusCode(
            201,
            new DynamicClientRegistrationResponse(
                clientId,
                clientSecret,
                client.Name,
                client.RedirectUris,
                GrantTypes: ["authorization_code"],
                ResponseTypes: ["code"],
                TokenEndpointAuthMethod: "client_secret_post"
            )
        );
    }

    private static string HashSecret(string secret)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
