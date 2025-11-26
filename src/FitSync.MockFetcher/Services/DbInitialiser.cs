namespace FitSync.MockFetcher.Services;

using System.Data;
using BCrypt.Net;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.MockFetcher.Configuration;
using FitSync.Shared.Extensions;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

/// <summary>
/// Initialises the database with mock data when running the mock fetcher.
/// </summary>
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

    /// <summary>
    /// Initialises the database with mock data when running the mock fetcher.
    /// </summary>
    public async Task MigrateAndSeedDatabase()
    {
        this.logger.LogInformation("Ensuring database is clean...");
        await fitSyncDbContext.Database.EnsureDeletedAsync();

        this.logger.LogInformation("Running migrations...");
        await fitSyncDbContext.Database.MigrateAsync();
        this.logger.LogInformation("Migrations completed successfully");
        this.logger.LogInformation("Seeding database");
        using var transaction = await fitSyncDbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable
        );

        User mockUser =
            new()
            {
                Username = "default",
                Email = "default@fitsync.com",
                EmailHash = "default@fitsync.com".SHA256Hashed(),
                PasswordHash = BCrypt.HashPassword(SharedPassword),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

        // Encrypt email before saving
        mockUser.Encrypt(this.encryptionService);

        await fitSyncDbContext.Users.AddAsync(mockUser);
        await fitSyncDbContext.SaveChangesAsync();
        this.logger.LogInformation("Added mock fetcher user: default");

        // Add Garmin credentials for the mock user to enable uploading
        if (
            !await fitSyncDbContext.UserCredentials.AnyAsync(
                g => g.UserId == mockUser.Id && g.ServiceType == ServiceTypes.Garmin
            )
        )
        {
            UserCredential garminCredential =
                new()
                {
                    UserId = mockUser.Id,
                    ServiceType = ServiceTypes.Garmin,
                    Username = this.options.Value.GarminConnectEmail ?? mockUser.Email,
                    Password = this.options.Value.GarminConnectPassword ?? SharedPassword,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

            garminCredential.Encrypt(this.encryptionService);

            await fitSyncDbContext.UserCredentials.AddAsync(garminCredential);
            await fitSyncDbContext.SaveChangesAsync();
            this.logger.LogInformation("Added Garmin credentials for default");
        }

        await transaction.CommitAsync();
        this.logger.LogInformation("Seeded database successfully");

        this.healthCheck.MarkAsHealthy();
        this.logger.LogInformation(
            "Database initialization complete - health check marked as healthy"
        );
    }
}
