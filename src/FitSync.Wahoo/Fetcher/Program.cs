using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Shared.Extensions;
using FitSync.Shared.Features.Encryption;
using FitSync.Shared.Features.GlobalVariables;
using FitSync.Shared.Features.Heartbeat;
using FitSync.Shared.Features.RateLimiting;
using FitSync.Wahoo.Fetcher.Configuration;
using FitSync.Wahoo.Fetcher.Features.WahooFetcher;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilog();

builder
    .Services.AddOptions<WahooFetcherOptions>()
    .BindConfiguration("WahooFetcherOptions")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddDbContext<FitSyncDbContext>(
    options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("FitSync")
        )
);

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    ConfigurationOptions configuration = ConfigurationOptions.Parse(
        builder.Configuration.GetConnectionString("Redis") ?? string.Empty
    );
    configuration.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(configuration);
});

WahooFetcherOptions fetcherConfig =
    builder.Configuration.GetSection("WahooFetcherOptions").Get<WahooFetcherOptions>()
    ?? throw new ArgumentException("Configuration section 'WahooFetcherOptions' is required.");

builder.Services.AddGlobalVariables(
    fetcherConfig.InstanceId,
    Environment.MachineName,
    fetcherConfig.HeartbeatIntervalMinutes,
    ServiceType.WahooFetcher
);

builder.AddKafkaProducer<string, string>("kafka");

builder
    .Services.AddEncryptionService(() => builder.Configuration.GetSection("DataProtectionOptions"))
    .AddWahooFetcher(() => builder.Configuration.GetSection("WahooFetcherOptions"))
    .AddHeartbeat()
    .AddRateLimiting();

var app = builder.Build();

app.Run();
