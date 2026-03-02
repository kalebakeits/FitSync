namespace FitSync.Api.Features.Credentials.Controllers;

using FitSync.Api.Features.Credentials.DTOs;
using FitSync.Api.Features.Credentials.Services;
using FitSync.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CredentialsController(
    ICredentialsService credentialsService,
    ICurrentUserService currentUserService,
    ILogger<CredentialsController> logger
) : ControllerBase
{
    private readonly ICredentialsService credentialsService = credentialsService;
    private readonly ICurrentUserService currentUserService = currentUserService;
    private readonly ILogger<CredentialsController> logger = logger;

    [HttpPost]
    public async Task<ActionResult<CredentialResponse>> CreateOrUpdateCredential(
        [FromBody] CreateCredentialRequest request,
        CancellationToken cancellationToken
    )
    {
        Guid userId = this.currentUserService.GetUserId();
        CredentialResponse credential = await this.credentialsService.CreateOrUpdateCredentialAsync(
            userId,
            request,
            cancellationToken
        );
        this.logger.LogInformation("Credential upserted for {ServiceType} user {UserId}.", request.ServiceType, userId);
        return this.Ok(credential);
    }

    [HttpGet]
    public async Task<ActionResult<List<CredentialResponse>>> GetCredentials(
        CancellationToken cancellationToken
    )
    {
        Guid userId = this.currentUserService.GetUserId();
        List<CredentialResponse> credentials = await this.credentialsService.GetCredentialsAsync(
            userId,
            cancellationToken
        );
        return this.Ok(credentials);
    }

    [HttpDelete("{serviceType}")]
    public async Task<ActionResult> DeleteCredential(
        string serviceType,
        CancellationToken cancellationToken
    )
    {
        Guid userId = this.currentUserService.GetUserId();
        await this.credentialsService.DeleteCredentialAsync(userId, serviceType, cancellationToken);
        return this.NoContent();
    }

    [HttpGet("available")]
    public async Task<ActionResult<List<AvailableServiceResponse>>> GetAvailableServices(
        CancellationToken cancellationToken
    )
    {
        Guid userId = this.currentUserService.GetUserId();
        List<AvailableServiceResponse> available = await this.credentialsService.GetAvailableServicesAsync(
            userId,
            cancellationToken
        );
        return this.Ok(available);
    }
}
