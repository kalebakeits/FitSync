namespace FitSync.Api.Features.Credentials.Services;

using FitSync.Api.Configurations;
using FitSync.Api.Exceptions;
using FitSync.Api.Features.Credentials.DTOs;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class CredentialsService(
    FitSyncDbContext context,
    IEncryptionService encryptionService,
    IOptions<AppConfiguration> appConfiguration,
    ServiceCredentialHandlerFactory handlerFactory,
    IEnumerable<IOAuthServiceHandler> oauthHandlers,
    ILogger<CredentialsService> logger
) : ICredentialsService
{
    private readonly FitSyncDbContext context = context;
    private readonly IEncryptionService encryptionService = encryptionService;
    private readonly ILogger<CredentialsService> logger = logger;
    private readonly IOptions<AppConfiguration> appConfiguration = appConfiguration;
    private readonly ServiceCredentialHandlerFactory handlerFactory = handlerFactory;
    private readonly IEnumerable<IOAuthServiceHandler> oauthHandlers = oauthHandlers;

    public async Task<CredentialResponse> CreateOrUpdateCredentialAsync(
        Guid userId,
        CreateCredentialRequest request,
        CancellationToken cancellationToken = default
    )
    {
        IServiceCredentialHandler handler = this.handlerFactory.Require(request.ServiceType);

        Integration? existing = await this.context.Integrations.FirstOrDefaultAsync(
            i => i.UserId == userId && i.ServiceType == request.ServiceType,
            cancellationToken
        );

        if (existing != null)
        {
            existing.SetAuthData(handler.BuildAuthData(request), this.encryptionService);
            existing.FailureCount = 0;
            await this.context.SaveChangesAsync(cancellationToken);
            await handler.OnCredentialCreatedAsync(existing, cancellationToken);
            this.logger.LogInformation("Updated integration for {ServiceType} user {UserId}.", request.ServiceType, userId);
            return this.MapToResponse(existing, request.Username);
        }

        Integration integration = new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ServiceType = request.ServiceType,
            FailureCount = 0,
        };
        integration.SetAuthData(handler.BuildAuthData(request), this.encryptionService);
        this.context.Integrations.Add(integration);
        await this.context.SaveChangesAsync(cancellationToken);
        await handler.OnCredentialCreatedAsync(integration, cancellationToken);

        this.logger.LogInformation("Created integration for {ServiceType} user {UserId}.", request.ServiceType, userId);
        return this.MapToResponse(integration, request.Username);
    }

    public async Task<List<CredentialResponse>> GetCredentialsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        List<string> credentialServiceTypes = this.handlerFactory.ServiceTypes;

        List<Integration> integrations = await this.context.Integrations
            .Where(i => i.UserId == userId && credentialServiceTypes.Contains(i.ServiceType))
            .ToListAsync(cancellationToken);

        return integrations.Select(i =>
        {
            string? displayName = this.handlerFactory.Require(i.ServiceType).GetDisplayName(i);
            return this.MapToResponse(i, displayName);
        }).ToList();
    }

    public async Task DeleteCredentialAsync(
        Guid userId,
        string serviceType,
        CancellationToken cancellationToken = default
    )
    {
        Integration? integration = await this.context.Integrations.FirstOrDefaultAsync(
            i => i.UserId == userId && i.ServiceType == serviceType,
            cancellationToken
        );

        if (integration == null)
            throw new NotFoundException("Credential not found.");

        this.context.Integrations.Remove(integration);
        await this.context.SaveChangesAsync(cancellationToken);

        IServiceCredentialHandler? handler = this.handlerFactory.Get(serviceType);
        if (handler != null)
            await handler.OnCredentialDeletedAsync(integration, cancellationToken);

        this.logger.LogInformation("Deleted integration for {ServiceType} user {UserId}.", serviceType, userId);
    }

    public async Task<List<AvailableServiceResponse>> GetAvailableServicesAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        List<string> credentialServiceTypes = this.handlerFactory.ServiceTypes;
        List<string> oauthServiceTypes = this.oauthHandlers.Select(h => h.ServiceType).ToList();
        List<string> allServiceTypes = [.. credentialServiceTypes, .. oauthServiceTypes];

        List<string> existing = await this.context.Integrations
            .Where(i => i.UserId == userId && allServiceTypes.Contains(i.ServiceType))
            .Select(i => i.ServiceType)
            .ToListAsync(cancellationToken);

        List<AvailableServiceResponse> available = [];

        foreach (string serviceType in credentialServiceTypes.Where(s => !existing.Contains(s)))
        {
            available.Add(new AvailableServiceResponse
            {
                ServiceType = serviceType,
                AuthType = "credentials",
                ConnectUrl = null,
            });
        }

        foreach (IOAuthServiceHandler oauth in this.oauthHandlers.Where(h => !existing.Contains(h.ServiceType)))
        {
            available.Add(new AvailableServiceResponse
            {
                ServiceType = oauth.ServiceType,
                AuthType = oauth.AuthType,
                ConnectUrl = oauth.ConnectUrl,
            });
        }

        return available;
    }

    private CredentialResponse MapToResponse(Integration integration, string? username) =>
        new()
        {
            ServiceType = integration.ServiceType,
            Username = username ?? string.Empty,
            CreatedAt = integration.CreatedAt,
            UpdatedAt = integration.UpdatedAt,
            Enabled = integration.FailureCount < this.appConfiguration.Value.MaxSequentialCredentialFailures,
        };
}
