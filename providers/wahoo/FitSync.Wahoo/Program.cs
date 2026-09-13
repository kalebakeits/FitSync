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
using FitSync.Wahoo.Configuration;
using FitSync.Wahoo.Shared.WahooClient;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddSerilog();

builder
    .Services.AddOptions<WahooFetcherOptions>()
    .BindConfiguration("WahooFetcherOptions")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddDbContext<FitSyncDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("FitSync"))
);

WahooFetcherOptions fetcherConfig =
    builder.Configuration.GetSection("WahooFetcherOptions").Get<WahooFetcherOptions>()
    ?? throw new ArgumentException("Configuration section 'WahooFetcherOptions' is required.");

IReadOnlyList<HeartbeatRole> heartbeatRoles = fetcherConfig.Enabled
    ? [new HeartbeatRole(InstanceIdentity.Derive(fetcherConfig.InstanceId), ServiceType.WahooFetcher)]
    : [];

builder.Services.AddGlobalVariables(
    heartbeatRoles,
    InstanceIdentity.Derive(fetcherConfig.InstanceId),
    Environment.MachineName,
    fetcherConfig.HeartbeatIntervalMinutes,
    ServiceTypes.Wahoo
);

builder.AddKafkaProducer<string, string>("kafka");

builder
    .Services.AddEncryptionService(() => builder.Configuration.GetSection("DataProtectionOptions"))
    .AddWahooClient(() => builder.Configuration.GetSection("WahooFetcherOptions:Client"))
    .AddFetcher<WahooClient>(() => builder.Configuration.GetSection("WahooFetcherOptions"))
    // Bind is itself a Configure, so it runs first and this rewrites the value it bound.
    .Configure<FetcherOptions>(options =>
        options.InstanceId = InstanceIdentity.Derive(options.InstanceId)
    )
    .AddHeartbeat()
    .AddRateLimiting(builder.Configuration.GetConnectionString("Redis") ?? string.Empty)
    .AddKafkaTopicInitializer();

WebApplication app = builder.Build();

await app.EnsureKafkaTopicsAsync();

app.Run();
