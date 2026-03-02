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
    ICurrentUserService currentUserService,
    ILogger<ConnectionsController> logger
) : ControllerBase
{
    private readonly IConnectionsService connectionsService = connectionsService;
    private readonly ICurrentUserService currentUserService = currentUserService;
    private readonly ILogger<ConnectionsController> logger = logger;

    [HttpGet(Name = "GetApiConnections")]
    public async Task<ActionResult<List<ConnectionResponse>>> GetConnections(
        CancellationToken cancellationToken
    )
    {
        Guid userId = this.currentUserService.GetUserId();
        List<ConnectionResponse> connections = await this.connectionsService.GetConnectionsAsync(
            userId,
            cancellationToken
        );
        return this.Ok(connections);
    }

    [HttpDelete("{serviceType}", Name = "DeleteApiConnectionsServiceType")]
    public async Task<ActionResult> Disconnect(
        string serviceType,
        CancellationToken cancellationToken
    )
    {
        Guid userId = this.currentUserService.GetUserId();
        await this.connectionsService.DisconnectAsync(userId, serviceType, cancellationToken);
        this.logger.LogInformation("Disconnected {ServiceType} for user {UserId}.", serviceType, userId);
        return this.Ok();
    }
}
