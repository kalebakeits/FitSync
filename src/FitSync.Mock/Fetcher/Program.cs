using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using FitSync.Mock.Fetcher.Configuration;
using FitSync.Mock.Fetcher.Services;
using FitSync.Shared.Extensions;
using FitSync.Shared.Features.Encryption;
using FitSync.Shared.Features.Fetcher;
using FitSync.Shared.Features.GlobalVariables;
using FitSync.Shared.Features.Heartbeat;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.AddSerilog();

builder.Services.AddDbContext<FitSyncDbContext>(
    options =>
        options.UseNpgsql(
            builder.Configuration.GetSection("ConnectionStrings").GetValue<string>("FitSync")
        )
);

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(
        builder.Configuration.GetConnectionString("Redis") ?? string.Empty
    );
    configuration.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(configuration);
});
builder.Services.AddRateLimiter();

// Configuration
builder
    .Services.AddOptions<MockFetcherOptions>()
    .BindConfiguration("MockFetcherOptions")
    .ValidateDataAnnotations()
    .ValidateOnStart();

var mockConfig =
    builder.Configuration.GetSection("MockFetcherOptions").Get<MockFetcherOptions>()
    ?? throw new ArgumentException("Configuration section 'MockFetcherOptions' is required.");

// Add encryption service
builder.Services.AddEncryptionService(
    () => builder.Configuration.GetSection("DataProtectionOptions")
);

// Global variables
builder.Services.AddGlobalVariables(
    mockConfig.InstanceId,
    Environment.MachineName,
    mockConfig.HeartbeatIntervalMinutes,
    ServiceType.MockFetcher,
    ServiceTypes.Zwift
);

// Kafka producer
builder.AddKafkaProducer<string, string>("kafka");

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

await app.RunAsync();
