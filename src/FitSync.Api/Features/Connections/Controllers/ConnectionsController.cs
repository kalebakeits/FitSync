namespace FitSync.Api.Features.Connections.Controllers;

using FitSync.Api.Features.Connections.DTOs;
using FitSync.Api.Features.Connections.Services;
using FitSync.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/connections")]
[Authorize]
public class ConnectionsController(
    IConnectionsService connectionsService,
    IDestinationMappingService destinationMappingService,
    IFetcherStatusService fetcherStatusService,
    ICurrentUserService currentUserService,
    ILogger<ConnectionsController> logger
) : ControllerBase
{
    private readonly IConnectionsService connectionsService = connectionsService;
    private readonly IDestinationMappingService destinationMappingService = destinationMappingService;
    private readonly IFetcherStatusService fetcherStatusService = fetcherStatusService;
    private readonly ICurrentUserService currentUserService = currentUserService;
    private readonly ILogger<ConnectionsController> logger = logger;

    [HttpGet(Name = "GetApiConnections")]
    public async Task<ActionResult<List<ConnectionResponse>>> GetConnectionsAsync(
        CancellationToken cancellationToken
    )
    {
        Guid userId = this.currentUserService.GetUserId();
        return this.Ok(await this.connectionsService.GetConnectionsAsync(userId, cancellationToken));
    }

    [HttpDelete("{serviceType}", Name = "DeleteApiConnectionsServiceType")]
    public async Task<ActionResult> DisconnectAsync(
        string serviceType,
        CancellationToken cancellationToken
    )
    {
        Guid userId = this.currentUserService.GetUserId();
        await this.connectionsService.DisconnectAsync(userId, serviceType, cancellationToken);
        this.logger.LogInformation("Disconnected {ServiceType} for user {UserId}.", serviceType, userId);
        return this.Ok();
    }

    [HttpGet("mappings", Name = "GetApiConnectionsMappings")]
    public async Task<ActionResult<List<DestinationMappingResponse>>> GetMappingsAsync(
        CancellationToken cancellationToken
    )
    {
        Guid userId = this.currentUserService.GetUserId();
        return this.Ok(await this.destinationMappingService.GetMappingsAsync(userId, cancellationToken));
    }

    [HttpPut("mappings", Name = "PutApiConnectionsMappings")]
    public async Task<ActionResult> UpsertMappingsAsync(
        [FromBody] UpsertDestinationMappingsRequest request,
        CancellationToken cancellationToken
    )
    {
        Guid userId = this.currentUserService.GetUserId();
        await this.destinationMappingService.UpsertMappingsAsync(userId, request, cancellationToken);
        return this.Ok();
    }

    [HttpGet("status", Name = "GetApiConnectionsStatus")]
    public async Task<ActionResult<List<FetcherStatusResponse>>> GetStatusAsync(
        CancellationToken cancellationToken
    )
    {
        Guid userId = this.currentUserService.GetUserId();
        return this.Ok(await this.fetcherStatusService.GetStatusAsync(userId, cancellationToken));
    }
}
