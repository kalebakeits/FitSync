using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using FitSync.Shared.Extensions;
using FitSync.Shared.Features.Encryption;
using FitSync.Shared.Features.Fetcher;
using FitSync.Shared.Features.GlobalVariables;
using FitSync.Shared.Features.Heartbeat;
using FitSync.Shared.Features.RateLimiting;
using FitSync.Wahoo.Fetcher.Configuration;
using FitSync.Wahoo.Shared.WahooClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddGlobalVariables(
    fetcherConfig.InstanceId,
    Environment.MachineName,
    fetcherConfig.HeartbeatIntervalMinutes,
    ServiceType.WahooFetcher,
    ServiceTypes.Wahoo
);

builder.AddKafkaProducer<string, string>("kafka");

builder
    .Services.AddEncryptionService(() => builder.Configuration.GetSection("DataProtectionOptions"))
    .AddWahooClient(() => builder.Configuration.GetSection("WahooFetcherOptions:Client"))
    .AddFetcher<WahooClient>(() => builder.Configuration.GetSection("WahooFetcherOptions"))
    .AddHeartbeat()
    .AddRateLimiting(builder.Configuration.GetConnectionString("Redis") ?? string.Empty);

var app = builder.Build();

app.Run();
