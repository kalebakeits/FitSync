namespace FitSync.Mock.Fetcher.Services;

using System.Data;
using BCrypt.Net;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Garmin.Shared.AuthData;
using FitSync.Mock.Fetcher.Configuration;
using FitSync.Shared.Extensions;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class DbInitialiser(
    FitSyncDbContext fitSyncDbContext,
    IEncryptionService encryptionService,
    ILogger<DbInitialiser> logger,
    IOptions<MockFetcherOptions> options,
    DbInitializerHealthCheck healthCheck
)
{
    private readonly FitSyncDbContext fitSyncDbContext = fitSyncDbContext;
    private readonly IEncryptionService encryptionService = encryptionService;
    private readonly ILogger<DbInitialiser> logger = logger;
    private readonly IOptions<MockFetcherOptions> options = options;
    private readonly DbInitializerHealthCheck healthCheck = healthCheck;

    private const string SharedPassword = "default1";

    public async Task MigrateAndSeedDatabase()
    {
        string defaultEmail = "default@fitsync.com";
        this.logger.LogInformation("Ensuring database is clean...");
        await this.fitSyncDbContext.Database.EnsureDeletedAsync();

        this.logger.LogInformation("Running migrations...");
        await this.fitSyncDbContext.Database.MigrateAsync();
        this.logger.LogInformation("Migrations completed successfully");
        this.logger.LogInformation("Seeding database");

        using var transaction = await this.fitSyncDbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable
        );

        User mockUser =
            new()
            {
                Username = "default",
                Email = defaultEmail,
                EmailHash = defaultEmail.SHA256Hashed(),
                PasswordHash = BCrypt.HashPassword(SharedPassword),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

        mockUser.Encrypt(this.encryptionService);
        await this.fitSyncDbContext.Users.AddAsync(mockUser);
        await this.fitSyncDbContext.SaveChangesAsync();
        this.logger.LogInformation("Added mock fetcher user: default");

        bool garminExists = await this.fitSyncDbContext.Integrations.AnyAsync(
            i => i.UserId == mockUser.Id && i.ServiceType == ServiceTypes.Garmin
        );

        if (!garminExists)
        {
            Integration garminIntegration =
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = mockUser.Id,
                    ServiceType = ServiceTypes.Garmin,
                    FailureCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

            GarminAuthData authData =
                new()
                {
                    Username = this.options.Value.GarminConnectEmail ?? defaultEmail,
                    Password = this.options.Value.GarminConnectPassword ?? SharedPassword,
                };

            garminIntegration.SetAuthData(authData, this.encryptionService);
            await this.fitSyncDbContext.Integrations.AddAsync(garminIntegration);
            await this.fitSyncDbContext.SaveChangesAsync();
            this.logger.LogInformation("Added Garmin integration for default user.");
        }

        await transaction.CommitAsync();
        this.logger.LogInformation("Seeded database successfully");
        this.healthCheck.MarkAsHealthy();
        this.logger.LogInformation(
            "Database initialization complete - health check marked as healthy"
        );
    }
}
