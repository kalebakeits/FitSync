using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using FitSync.Mock.Fetcher.Configuration;
using FitSync.Mock.Fetcher.Services;
using FitSync.Shared.Configuration;
using FitSync.Shared.Extensions;
using FitSync.Shared.Features.Encryption;
using FitSync.Shared.Features.Fetcher;
using FitSync.Shared.Features.GlobalVariables;
using FitSync.Shared.Features.GlobalVariables.DTOs;
using FitSync.Shared.Features.Heartbeat;
using FitSync.Shared.Features.Kafka;
using FitSync.Shared.Features.RateLimiting;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.AddSerilog();

builder.Services.AddDbContext<FitSyncDbContext>(
    options =>
        options.UseNpgsql(
            builder.Configuration.GetSection("ConnectionStrings").GetValue<string>("FitSync")
        )
);

builder.Services.AddRateLimiting(
    builder.Configuration.GetConnectionString("Redis") ?? string.Empty
);

// Configuration
builder
    .Services.AddOptions<MockFetcherOptions>()
    .BindConfiguration("MockFetcherOptions")
    .ValidateDataAnnotations()
    .ValidateOnStart();

MockFetcherOptions mockConfig =
    builder.Configuration.GetSection("MockFetcherOptions").Get<MockFetcherOptions>()
    ?? throw new ArgumentException("Configuration section 'MockFetcherOptions' is required.");

// Add encryption service
builder.Services.AddEncryptionService(
    () => builder.Configuration.GetSection("DataProtectionOptions")
);

// Global variables
IReadOnlyList<HeartbeatRole> heartbeatRoles = mockConfig.RunFetcher
    ? [new HeartbeatRole(InstanceIdentity.Derive(mockConfig.InstanceId), ServiceType.MockFetcher)]
    : [];

builder.Services.AddGlobalVariables(
    heartbeatRoles,
    InstanceIdentity.Derive(mockConfig.InstanceId),
    Environment.MachineName,
    mockConfig.HeartbeatIntervalMinutes,
    ServiceTypes.Mock
);

// Kafka producer
builder.AddKafkaProducer<string, string>("kafka");
builder.Services.AddKafkaTopicInitializer();

// Health check for DB initialization
DbInitializerHealthCheck healthCheck = new();
builder.Services.AddSingleton(healthCheck);
builder.Services.AddHealthChecks().AddCheck("db-initializer", healthCheck);

// Features
IServiceCollection services = builder.Services;
services.AddScoped<DbInitialiser>();
services.AddHostedService<UserVerificationWorker>();

if (mockConfig.RunFetcher)
{
    services
        .AddFetcher<MockFetcherClient>(() => builder.Configuration.GetSection("MockFetcherOptions"))
        // Bind is itself a Configure, so it runs first and this rewrites the value it bound.
        .Configure<FetcherOptions>(options =>
            options.InstanceId = InstanceIdentity.Derive(options.InstanceId)
        )
        .AddHeartbeat();
}

WebApplication app = builder.Build();

ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();

if (mockConfig.RunFetcher)
{
    logger.LogInformation("Mock fetcher is enabled");
}
else
{
    logger.LogInformation("Mock fetcher is disabled - only running DB initialization");
}

// Map health check endpoint
app.MapHealthChecks("/health");

using (IServiceScope scope = app.Services.CreateScope())
{
    try
    {
        DbInitialiser dbInitialiser = scope.ServiceProvider.GetRequiredService<DbInitialiser>();
        await dbInitialiser.MigrateAndSeedDatabase();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database setup");
    }
}

await app.EnsureKafkaTopicsAsync();

await app.RunAsync();
