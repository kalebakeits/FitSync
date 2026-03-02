using Confluent.Kafka;
using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Shared.Extensions;
using FitSync.Shared.Features.Encryption;
using FitSync.Shared.Features.GlobalVariables;
using FitSync.Shared.Features.Heartbeat;
using FitSync.Shared.Features.RateLimiting;
using FitSync.Garmin.Uploader.Configuration;
using FitSync.Garmin.Uploader.Features.ActivityProcessing;
using FitSync.Garmin.Uploader.Features.FitModification;
using FitSync.Garmin.Uploader.Features.GarminUpload;
using FitSync.Garmin.Uploader.Features.Kafka;
using FitSync.Garmin.Uploader.Features.OrphanedWork;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Add Serilog
builder.AddSerilog();

// Configuration
builder
    .Services.AddOptions<GarminUploaderOptions>()
    .BindConfiguration("GarminUploaderOptions")
    .ValidateDataAnnotations()
    .ValidateOnStart();

var uploaderConfig = builder
    .Configuration.GetSection("GarminUploaderOptions")
    .Get<GarminUploaderOptions>()!;

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
builder.Services.AddGlobalVariables(
    uploaderConfig.InstanceId,
    Environment.MachineName,
    uploaderConfig.HeartbeatIntervalMinutes,
    ServiceType.GarminUploader
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

// Features
builder
    .Services.AddEncryptionService(() => builder.Configuration.GetSection("DataProtectionOptions"))
    .AddHeartbeat()
    .AddRateLimiting()
    .AddKafkaConsumer()
    .AddFitModification()
    .AddGarminUpload()
    .AddActivityProcessing()
    .AddOrphanedWorkReclaimer();

IHost host = builder.Build();

await host.RunAsync();
