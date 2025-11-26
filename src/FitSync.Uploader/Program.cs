using Confluent.Kafka;
using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.ServiceDefaults;
using FitSync.Shared.Extensions;
using FitSync.Shared.Features.Encryption;
using FitSync.Shared.Features.GlobalVariables;
using FitSync.Shared.Features.Heartbeat;
using FitSync.Uploader.Configuration;
using FitSync.Uploader.Features.ActivityProcessing;
using FitSync.Uploader.Features.FitModification;
using FitSync.Uploader.Features.GarminUpload;
using FitSync.Uploader.Features.Kafka;
using FitSync.Uploader.Features.OrphanedWork;
using Microsoft.EntityFrameworkCore;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

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
    .AddKafkaConsumer()
    .AddFitModification()
    .AddGarminUpload()
    .AddActivityProcessing()
    .AddOrphanedWorkReclaimer();

IHost host = builder.Build();

await host.RunAsync();
