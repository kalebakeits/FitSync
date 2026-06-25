using FitSync.Database;
using FitSync.Purger.Configuration;
using FitSync.Purger.Features.ActivityPurger;
using FitSync.Shared.Extensions;
using Microsoft.EntityFrameworkCore;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddSerilog();

builder
    .Services.AddOptions<PurgerOptions>()
    .BindConfiguration("PurgerOptions")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddDbContext<FitSyncDbContext>(
    options =>
        options.UseNpgsql(
            builder.Configuration.GetSection("ConnectionStrings").GetValue<string>("FitSync")
        )
);

builder.Services.AddActivityPurger();

IHost host = builder.Build();
await host.RunAsync();
