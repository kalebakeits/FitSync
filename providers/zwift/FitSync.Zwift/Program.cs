using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using FitSync.Shared.Configuration;
using FitSync.Shared.Extensions;
using FitSync.Shared.Features.Encryption;
using FitSync.Shared.Features.Fetcher;
using FitSync.Shared.Features.GlobalVariables;
using FitSync.Shared.Features.GlobalVariables.DTOs;
using FitSync.Shared.Features.Heartbeat;
using FitSync.Shared.Features.Kafka;
using FitSync.Shared.Features.RateLimiting;
using FitSync.Zwift.Shared.Configuration;
using FitSync.Zwift.Shared.ZwiftClient;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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

// Global variables
ZwiftFetcherOptions fetcherConfig =
    builder.Configuration.GetSection("ZwiftFetcherOptions").Get<ZwiftFetcherOptions>()
    ?? throw new ArgumentException("Configuration section 'ZwiftFetcherOptions' is required.");

IReadOnlyList<HeartbeatRole> heartbeatRoles = fetcherConfig.Enabled
    ? [new HeartbeatRole(InstanceIdentity.Derive(fetcherConfig.InstanceId), ServiceType.ZwiftFetcher)]
    : [];

builder.Services.AddGlobalVariables(
    heartbeatRoles,
    InstanceIdentity.Derive(fetcherConfig.InstanceId),
    Environment.MachineName,
    fetcherConfig.HeartbeatIntervalMinutes,
    ServiceTypes.Zwift
);

// Kafka producer
builder.AddKafkaProducer<string, string>("kafka");

// Features
builder
    .Services.AddEncryptionService(() => builder.Configuration.GetSection("DataProtectionOptions"))
    .AddFetcher<ZwiftClient>(() => builder.Configuration.GetSection("ZwiftFetcherOptions"))
    // Bind is itself a Configure, so it runs first and this rewrites the value it bound.
    .Configure<FetcherOptions>(options =>
        options.InstanceId = InstanceIdentity.Derive(options.InstanceId)
    )
    .AddZwiftClient()
    .AddHeartbeat()
    .AddRateLimiting(builder.Configuration.GetConnectionString("Redis") ?? string.Empty)
    .AddKafkaTopicInitializer();

WebApplication app = builder.Build();

await app.EnsureKafkaTopicsAsync();

app.Run();
