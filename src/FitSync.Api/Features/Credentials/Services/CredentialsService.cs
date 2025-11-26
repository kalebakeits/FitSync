using FitSync.Api.Configurations;
using FitSync.Api.Exceptions;
using FitSync.Api.Features.Credentials.DTOs;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FitSync.Api.Features.Credentials.Services;

public class CredentialsService(
    FitSyncDbContext context,
    IEncryptionService encryptionService,
    IOptions<AppConfiguration> appConfiguration,
    ServiceCredentialHandlerFactory handlerFactory,
    ILogger<CredentialsService> logger
) : ICredentialsService
{
    private readonly FitSyncDbContext context = context;
    private readonly IEncryptionService encryptionService = encryptionService;
    private readonly ILogger<CredentialsService> logger = logger;
    private readonly IOptions<AppConfiguration> appConfiguration = appConfiguration;
    private readonly ServiceCredentialHandlerFactory handlerFactory = handlerFactory;

    public async Task<CredentialResponse> CreateOrUpdateCredentialAsync(
        Guid userId,
        CreateCredentialRequest request
    )
    {
        this.logger.LogInformation(
            "Creating or updating credential for user: {UserId}, service: {ServiceType}",
            userId,
            request.ServiceType
        );

        UserCredential? existing = await this.context.UserCredentials.FirstOrDefaultAsync(
            c => c.UserId == userId && c.ServiceType == request.ServiceType
        );
        string plaintextUsername;
        IServiceCredentialHandler? handler;

        if (existing != null)
        {
            this.logger.LogInformation(
                "Updating existing credential for user: {UserId}, service: {ServiceType}",
                userId,
                request.ServiceType
            );

            existing.Username = request.Username;
            existing.Password = request.Password;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.FailureCount = 0; // Reset failure count when credentials are updated

            plaintextUsername = existing.Username;
            existing.Encrypt(this.encryptionService);

            await this.context.SaveChangesAsync();

            this.logger.LogInformation(
                "Credential updated successfully for user: {UserId}, service: {ServiceType}",
                userId,
                request.ServiceType
            );

            // Notify service-specific handler (in case config needs to be created/updated)
            handler = this.handlerFactory.GetHandler(request.ServiceType);
            if (handler != null)
            {
                await handler.OnCredentialCreatedAsync(userId);
            }

            return new CredentialResponse
            {
                ServiceType = existing.ServiceType,
                Username = plaintextUsername,
                CreatedAt = existing.CreatedAt,
                UpdatedAt = existing.UpdatedAt,
                Enabled =
                    existing.FailureCount < appConfiguration.Value.MaxSequentialCredentialFailures,
            };
        }

        this.logger.LogInformation(
            "Creating new credential for user: {UserId}, service: {ServiceType}",
            userId,
            request.ServiceType
        );

        UserCredential credential =
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ServiceType = request.ServiceType,
                Username = request.Username,
                Password = request.Password,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                FailureCount = 0 // Initialize failure count to 0
            };

        plaintextUsername = credential.Username;
        credential.Encrypt(this.encryptionService);

        this.context.UserCredentials.Add(credential);
        await this.context.SaveChangesAsync();

        this.logger.LogInformation(
            "Credential created successfully for user: {UserId}, service: {ServiceType}",
            userId,
            request.ServiceType
        );

        handler = this.handlerFactory.GetHandler(request.ServiceType);
        if (handler != null)
        {
            await handler.OnCredentialCreatedAsync(userId);
        }

        return new CredentialResponse
        {
            ServiceType = credential.ServiceType,
            Username = plaintextUsername,
            CreatedAt = credential.CreatedAt,
            UpdatedAt = credential.UpdatedAt,
            Enabled =
                credential.FailureCount < appConfiguration.Value.MaxSequentialCredentialFailures,
        };
    }

    public async Task<List<CredentialResponse>> GetCredentialsAsync(Guid userId)
    {
        this.logger.LogInformation("Getting credentials for user: {UserId}", userId);

        List<UserCredential> credentials = await this.context.UserCredentials.Where(
            c => c.UserId == userId
        )
            .ToListAsync();

        this.logger.LogInformation(
            "Retrieved {Count} credentials for user: {UserId}",
            credentials.Count,
            userId
        );

        return credentials
            .Select(c =>
            {
                return new CredentialResponse
                {
                    ServiceType = c.ServiceType,
                    Username = c.Decrypt(this.encryptionService).Username,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    Enabled =
                        c.FailureCount < appConfiguration.Value.MaxSequentialCredentialFailures,
                };
            })
            .ToList();
    }

    public async Task DeleteCredentialAsync(Guid userId, string serviceType)
    {
        this.logger.LogInformation(
            "Deleting credential for user: {UserId}, service: {ServiceType}",
            userId,
            serviceType
        );

        UserCredential? credential = await this.context.UserCredentials.FirstOrDefaultAsync(
            c => c.UserId == userId && c.ServiceType == serviceType
        );

        if (credential == null)
        {
            this.logger.LogWarning(
                "Credential not found for deletion - user: {UserId}, service: {ServiceType}",
                userId,
                serviceType
            );
            throw new NotFoundException("Credential not found.");
        }

        this.context.UserCredentials.Remove(credential);
        await this.context.SaveChangesAsync();

        this.logger.LogInformation(
            "Credential deleted successfully for user: {UserId}, service: {ServiceType}",
            userId,
            serviceType
        );

        // Notify service-specific handler
        IServiceCredentialHandler? handler = this.handlerFactory.GetHandler(serviceType);
        if (handler != null)
        {
            await handler.OnCredentialDeletedAsync(userId);
        }
    }

    public async Task<List<string>> GetAvailableServicesAsync(Guid userId)
    {
        this.logger.LogInformation("Getting available services for user: {UserId}", userId);

        List<string> allServices = [ServiceTypes.Zwift, ServiceTypes.Garmin];

        List<string> existingServices = await this.context.UserCredentials.Where(
            c => c.UserId == userId
        )
            .Select(c => c.ServiceType)
            .ToListAsync();

        List<string> availableServices = allServices
            .Where(s => !existingServices.Contains(s))
            .ToList();

        this.logger.LogInformation(
            "Found {Count} available services for user: {UserId}",
            availableServices.Count,
            userId
        );

        return availableServices;
    }
}
