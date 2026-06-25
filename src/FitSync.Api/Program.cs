using System.Text.Json.Serialization;
using Confluent.Kafka;
using FitSync.Api.Configurations;
using FitSync.Api.Features.Account;
using FitSync.Api.Features.Activities;
using FitSync.Api.Features.Auth;
using FitSync.Api.Features.Connections;
using FitSync.Api.Features.Credentials;
using FitSync.Api.Features.Fetchers;
using FitSync.Api.Features.Garmin;
using FitSync.Api.Features.Mcp;
using FitSync.Api.Features.OAuth;
using FitSync.Api.Features.Profile;
using FitSync.Api.Features.Tokens;
using FitSync.Api.Features.TrainingProfile;
using FitSync.Api.Features.Wahoo;
using FitSync.Api.Features.WorkoutPublishing;
using FitSync.Api.Features.Workouts;
using FitSync.Api.Middleware;
using FitSync.Api.Services;
using FitSync.Database;
using FitSync.Shared.Features.Email;
using FitSync.Shared.Features.Email.Services;
using FitSync.Shared.Features.Encryption;
using FitSync.Shared.Features.RateLimiting;
using FitSync.Shared.Features.WorkoutBuilder;
using FitSync.Shared.Features.WorkoutPublisher;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddOptions<AppConfiguration>()
    .BindConfiguration("AppConfiguration")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder
    .Services.AddOptions<EmailConfiguration>()
    .BindConfiguration("EmailConfiguration")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder
    .Services.AddOptions<WahooOptions>()
    .BindConfiguration("WahooOptions")
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Configure Serilog
builder.Host.UseSerilog(
    (context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
);

builder.Services.AddSingleton<IProducer<string, string>>(_ =>
{
    ProducerConfig config =
        new()
        {
            BootstrapServers = builder.Configuration.GetConnectionString("kafka"),
            MessageTimeoutMs = 10000,
            MetadataMaxAgeMs = 86400000, // 24h max — static single-broker environment
        };
    return new ProducerBuilder<string, string>(config).Build();
});

// Add DbContext
builder.Services.AddDbContext<FitSyncDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("FitSync"))
);

// Add features
builder
    .Services.AddSwaggerGen(o => o.SupportNonNullableReferenceTypes())
    .AddEncryptionService(() => builder.Configuration.GetSection("DataProtectionOptions"))
    .AddRateLimiting(builder.Configuration.GetConnectionString("Redis") ?? string.Empty)
    .AddEmailService()
    .AddHttpContextAccessor()
    .AddScoped<ISessionService, SessionService>()
    .AddScoped<ICurrentUserService, CurrentUserService>()
    .AddAccountFeature()
    .AddAuthFeature()
    .AddProfileFeature()
    .AddCredentialsFeature()
    .AddConnectionsFeature()
    .AddActivitiesFeature()
    .AddWorkoutBuilderFeature()
    .AddWorkoutsFeature()
    .AddWorkoutPublisherFeature()
    .AddWorkoutPublishingFeature()
    .AddTrainingProfileFeature()
    .AddFetchersFeature()
    .AddWahooFeature(() => builder.Configuration.GetSection("WahooOptions"))
    .AddGarminFeature()
    .AddTokensFeature()
    .AddOAuthFeature()
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly()
    .Services.AddEndpointsApiExplorer()
    .AddAuthorization()
    .AddControllers(o => o.Conventions.Add(new RoutePrefixConvention("api")))
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Configure cookie authentication
builder
    .Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "FitSync.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
    });

string[] allowedOrigins =
    builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger().UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>()
    .UseCors()
    .UseHttpsRedirection()
    .UseSerilogRequestLogging()
    .UseMiddleware<ApiKeyAuthenticationMiddleware>()
    .UseMiddleware<SessionAuthenticationMiddleware>()
    .UseAuthentication()
    .UseAuthorization();
app.MapControllers();
app.MapMcp("/mcp");

app.Run();
