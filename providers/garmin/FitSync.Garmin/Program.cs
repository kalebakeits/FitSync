using Confluent.Kafka;
using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using FitSync.Garmin.Configuration;
using FitSync.Garmin.Features.ActivityProcessing;
using FitSync.Garmin.Features.FitModification;
using FitSync.Garmin.Features.GarminUpload;
using FitSync.Garmin.Features.Kafka;
using FitSync.Garmin.Features.OrphanedWork;
using FitSync.Garmin.Shared.Configuration;
using FitSync.Garmin.Shared.GarminClient;
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

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Add Serilog
builder.AddSerilog();

// Configuration
builder
    .Services.AddOptions<GarminUploaderOptions>()
    .BindConfiguration("GarminUploaderOptions")
    .ValidateDataAnnotations()
    .ValidateOnStart();

GarminFetcherOptions fetcherConfig = builder
    .Configuration.GetSection("GarminFetcherOptions")
    .Get<GarminFetcherOptions>()!;

if (fetcherConfig.Enabled)
{
    builder
        .Services.AddOptions<GarminFetcherOptions>()
        .BindConfiguration("GarminFetcherOptions")
        .ValidateDataAnnotations()
        .ValidateOnStart();
}

GarminUploaderOptions uploaderConfig = builder
    .Configuration.GetSection("GarminUploaderOptions")
    .Get<GarminUploaderOptions>()!;

builder.Services.AddDbContext<FitSyncDbContext>(
    options =>
        options.UseNpgsql(
            builder.Configuration.GetSection("ConnectionStrings").GetValue<string>("FitSync")
        )
);

builder.Services.AddDbContextFactory<FitSyncDbContext>(
    options =>
        options.UseNpgsql(
            builder.Configuration.GetSection("ConnectionStrings").GetValue<string>("FitSync")
        )
);

// Global variables
List<HeartbeatRole> heartbeatRoles = [];
if (uploaderConfig.Enabled)
{
    heartbeatRoles.Add(
        new HeartbeatRole(
            InstanceIdentity.Derive(uploaderConfig.InstanceId),
            ServiceType.GarminUploader
        )
    );
}

if (fetcherConfig.Enabled)
{
    heartbeatRoles.Add(
        new HeartbeatRole(
            InstanceIdentity.Derive(fetcherConfig.InstanceId),
            ServiceType.GarminFetcher
        )
    );
}

builder.Services.AddGlobalVariables(
    heartbeatRoles,
    InstanceIdentity.Derive(uploaderConfig.InstanceId),
    Environment.MachineName,
    uploaderConfig.Enabled
        ? uploaderConfig.HeartbeatIntervalMinutes
        : fetcherConfig.HeartbeatIntervalMinutes,
    ServiceTypes.Garmin
);

// Kafka consumer
builder.AddKafkaConsumer<string, string>(
    "kafka",
    settings =>
    {
        settings.Config.GroupId = "fitsync-uploader";
        settings.Config.AutoOffsetReset = AutoOffsetReset.Earliest;
        settings.Config.EnableAutoCommit = false;
    }
);

// Kafka producer
builder.AddKafkaProducer<string, string>("kafka");

// Features
IServiceCollection services = builder.Services;

services
    .AddEncryptionService(() => builder.Configuration.GetSection("DataProtectionOptions"))
    .AddHeartbeat()
    .AddRateLimiting(builder.Configuration.GetConnectionString("Redis") ?? string.Empty)
    .AddGarminClient()
    .AddKafkaTopicInitializer();

// The uploader half is left out of the container when disabled; AddFetcher does the same for the
// fetcher off GarminFetcherOptions:Enabled.
if (uploaderConfig.Enabled)
{
    services
        .AddKafkaConsumer()
        .AddFitModification()
        .AddGarminUpload()
        .AddActivityProcessing()
        .AddOrphanedWorkReclaimer();
}

// Bind is itself a Configure, so it runs first and this rewrites the value it bound.
services
    .AddFetcher<GarminActivityClient>(
        () => builder.Configuration.GetSection("GarminFetcherOptions")
    )
    .Configure<FetcherOptions>(options =>
        options.InstanceId = InstanceIdentity.Derive(options.InstanceId)
    );

IHost host = builder.Build();

await host.EnsureKafkaTopicsAsync();

await host.RunAsync();
