namespace FitSync.Api.Features.Connections.Services;

using FitSync.Api.Configurations;
using FitSync.Api.Features.Connections.DTOs;
using FitSync.Api.Features.Credentials.Services;
using FitSync.Database;
using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class ConnectionsService(
    FitSyncDbContext context,
    ServiceCredentialHandlerFactory handlerFactory,
    IEnumerable<IOAuthServiceHandler> oauthHandlers,
    IOptions<AppConfiguration> appConfiguration,
    ILogger<ConnectionsService> logger
) : IConnectionsService
{
    private readonly FitSyncDbContext context = context;
    private readonly ServiceCredentialHandlerFactory handlerFactory = handlerFactory;
    private readonly Dictionary<string, IOAuthServiceHandler> oauthHandlerMap =
        oauthHandlers.ToDictionary(h => h.ServiceType, h => h);
    private readonly IOptions<AppConfiguration> appConfiguration = appConfiguration;
    private readonly ILogger<ConnectionsService> logger = logger;

    public async Task<List<ConnectionResponse>> GetConnectionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        List<Integration> integrations = await this.context.Integrations
            .Where(i => i.UserId == userId)
            .ToListAsync(cancellationToken);

        return integrations.Select(i => this.MapToResponse(i)).ToList();
    }

    public async Task DisconnectAsync(
        Guid userId,
        string serviceType,
        CancellationToken cancellationToken = default
    )
    {
        Integration? integration = await this.context.Integrations
            .Include(i => i.FetcherConfig)
            .FirstOrDefaultAsync(
                i => i.UserId == userId && i.ServiceType == serviceType,
                cancellationToken
            );

        if (integration == null)
        {
            return;
        }

        this.context.Integrations.Remove(integration);
        await this.context.SaveChangesAsync(cancellationToken);
        this.logger.LogInformation("Disconnected {ServiceType} for user {UserId}.", serviceType, userId);
    }

    private ConnectionResponse MapToResponse(Integration integration)
    {
        bool enabled = integration.FailureCount
            < this.appConfiguration.Value.MaxSequentialCredentialFailures;

        string authType = this.oauthHandlerMap.ContainsKey(integration.ServiceType)
            ? "oauth"
            : "credentials";

        return new ConnectionResponse
        {
            ServiceType = integration.ServiceType,
            AuthType = authType,
            Connected = true,
            Enabled = enabled,
            DisplayName = this.ResolveDisplayName(integration),
            UpdatedAt = integration.UpdatedAt,
        };
    }

    private string? ResolveDisplayName(Integration integration)
    {
        IServiceCredentialHandler? credHandler = this.handlerFactory.Get(integration.ServiceType);
        if (credHandler != null)
            return credHandler.GetDisplayName(integration);

        this.oauthHandlerMap.TryGetValue(integration.ServiceType, out IOAuthServiceHandler? oauthHandler);
        return oauthHandler?.GetDisplayName(integration);
    }
}
