using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Shared.Extensions;
using FitSync.Shared.Features.Encryption;
using FitSync.Shared.Features.GlobalVariables;
using FitSync.Shared.Features.Heartbeat;
using FitSync.Shared.Features.RateLimiting;
using FitSync.ZwiftFetcher.Configuration;
using FitSync.ZwiftFetcher.Features.ZwiftClient;
using FitSync.ZwiftFetcher.Features.ZwiftFetcher;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.AddSerilog();

// Configuration
builder
    .Services.AddOptions<ZwiftFetcherOptions>()
    .BindConfiguration("ZwiftFetcherOptions")
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Add FitSync Context
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

// Global variables
var fetcherConfig =
    builder.Configuration.GetSection("ZwiftFetcherOptions").Get<ZwiftFetcherOptions>()
    ?? throw new ArgumentException("Configuration section 'ZwiftFetcherOptions' is required.");

builder.Services.AddGlobalVariables(
    fetcherConfig.InstanceId,
    Environment.MachineName,
    fetcherConfig.HeartbeatIntervalMinutes,
    ServiceType.ZwiftFetcher
);

// Kafka producer
builder.AddKafkaProducer<string, string>("kafka");

// Features
builder
    .Services.AddEncryptionService(() => builder.Configuration.GetSection("DataProtectionOptions"))
    .AddZwiftFetcher(() => builder.Configuration.GetSection("ZwiftFetcherOptions"))
    .AddZwiftClient()
    .AddHeartbeat()
    .AddRateLimiting();

var app = builder.Build();

app.Run();
