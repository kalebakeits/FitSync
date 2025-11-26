using FitSync.Api.Features.Credentials.DTOs;
using FitSync.Api.Features.Credentials.Services;
using FitSync.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitSync.Api.Features.Credentials.Controllers;

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
        [FromBody] CreateCredentialRequest request
    )
    {
        if (this.logger.IsEnabled(LogLevel.Information))
        {
            this.logger.LogInformation(
                "CreateOrUpdateCredential called for service: {ServiceType}",
                request.ServiceType
            );
        }

        Guid userId = this.currentUserService.GetUserId();
        CredentialResponse credential = await this.credentialsService.CreateOrUpdateCredentialAsync(
            userId,
            request
        );

        this.logger.LogInformation(
            "Credential created/updated for user: {UserId}, service: {ServiceType}",
            userId,
            request.ServiceType
        );
        return Ok(credential);
    }

    [HttpGet]
    public async Task<ActionResult<List<CredentialResponse>>> GetCredentials()
    {
        this.logger.LogInformation("GetCredentials called");

        Guid userId = this.currentUserService.GetUserId();
        List<CredentialResponse> credentials = await this.credentialsService.GetCredentialsAsync(
            userId
        );

        this.logger.LogInformation(
            "Retrieved {Count} credentials for user: {UserId}",
            credentials.Count,
            userId
        );
        return Ok(credentials);
    }

    [HttpDelete("{serviceType}")]
    public async Task<ActionResult> DeleteCredential(string serviceType)
    {
        this.logger.LogInformation(
            "DeleteCredential called for service: {ServiceType}",
            serviceType
        );

        Guid userId = this.currentUserService.GetUserId();
        await this.credentialsService.DeleteCredentialAsync(userId, serviceType);

        this.logger.LogInformation(
            "Credential deleted successfully for user: {UserId}, service: {ServiceType}",
            userId,
            serviceType
        );
        return NoContent();
    }

    [HttpGet("available")]
    public async Task<ActionResult<List<string>>> GetAvailableServices()
    {
        this.logger.LogInformation("GetAvailableServices called");

        Guid userId = this.currentUserService.GetUserId();
        List<string> availableServices = await this.credentialsService.GetAvailableServicesAsync(
            userId
        );

        this.logger.LogInformation(
            "Retrieved {Count} available services for user: {UserId}",
            availableServices.Count,
            userId
        );
        return Ok(availableServices);
    }
}
