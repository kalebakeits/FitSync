using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Projects;

// Pass the connection string directly with the fixed port
string connectionString =
    "Host=localhost;Port=5432;Database=FitSync;Username=postgres;Password=postgres";
string dataProtectionKey = "dev-encryption-key-change-in-production-12345";
string smtpPassword = "dev-smtp-password";

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL Database with a fixed host port
IResourceBuilder<ParameterResource> username = builder.AddParameter(
    "username",
    value: "postgres",
    secret: true
);
IResourceBuilder<ParameterResource> password = builder.AddParameter(
    "password",
    value: "postgres",
    secret: true
);

DateTime start = DateTime.Now.AddSeconds(10);
var healthCheckCallback = () =>
{
    return DateTime.Now > start ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
};
builder
    .Services.AddHealthChecks()
    .AddCheck("pgsql-health", healthCheckCallback)
    .AddCheck("kafka-health", healthCheckCallback);

IResourceBuilder<PostgresServerResource> postgres = builder
    .AddPostgres("postgres", username, password)
    .WithDataVolume()
    .WithPgAdmin()
    .WithHostPort(5432)
    .WithHealthCheck("pgsql-health");

IResourceBuilder<PostgresDatabaseResource> fitsyncDb = postgres.AddDatabase("FitSync");

// Kafka
IResourceBuilder<KafkaServerResource> kafka = builder
    .AddKafka("kafka")
    .WithDataVolume()
    .WithHealthCheck("kafka-health")
    .WithKafkaUI();

// Redis
IResourceBuilder<RedisResource> redis = builder.AddRedis("redis");

// Mock fetcher handles DB initialization and optionally runs the fetcher
// Other services wait for it to ensure DB is ready
IResourceBuilder<ProjectResource> mockFetcher = builder
    .AddProject<FitSync_Mock_Fetcher>("mock-fetcher")
    .WithReference(fitsyncDb)
    .WithReference(kafka)
    .WithEnvironment("ConnectionStrings__FitSync", connectionString)
    .WithEnvironment("DataProtectionOptions__DataProtectionKey", dataProtectionKey)
    .WithHttpEndpoint(port: 5100, name: "http")
    .WithHttpHealthCheck("/health")
    .WaitFor(fitsyncDb)
    .WaitFor(kafka);

builder
    .AddProject<FitSync_Zwift_Fetcher>("zwift-fetcher")
    .WithReference(fitsyncDb)
    .WithReference(kafka)
    .WithReference(redis)
    .WithEnvironment("ConnectionStrings__FitSync", connectionString)
    .WithEnvironment("DataProtectionOptions__DataProtectionKey", dataProtectionKey)
    .WaitFor(mockFetcher);

builder
    .AddProject<FitSync_Garmin_Uploader>("garmin-uploader")
    .WithReference(fitsyncDb)
    .WithReference(kafka)
    .WithReference(redis)
    .WithEnvironment("ConnectionStrings__FitSync", connectionString)
    .WithEnvironment("DataProtectionOptions__DataProtectionKey", dataProtectionKey)
    .WithReplicas(2)
    .WaitFor(mockFetcher);

builder
    .AddProject<FitSync_Wahoo_Fetcher>("wahoo-fetcher")
    .WithReference(fitsyncDb)
    .WithReference(kafka)
    .WithReference(redis)
    .WithEnvironment("ConnectionStrings__FitSync", connectionString)
    .WithEnvironment("DataProtectionOptions__DataProtectionKey", dataProtectionKey)
    .WaitFor(mockFetcher);

// API
IResourceBuilder<ProjectResource> api = builder
    .AddProject<FitSync_Api>("api")
    .WithReference(fitsyncDb)
    .WithReference(kafka)
    .WithReference(redis)
    .WithEnvironment("ConnectionStrings__FitSync", connectionString)
    .WithEnvironment("DataProtectionOptions__DataProtectionKey", dataProtectionKey)
    .WithEnvironment("EmailConfiguration__SmtpPassword", smtpPassword)
    .WithEnvironment(
        "Workouts__StoragePath",
        Path.Combine(Path.GetTempPath(), "fitsync-workout-files")
    )
    .WaitFor(mockFetcher);

// GUI (React + Vite)
builder
    .AddNpmApp("gui", "../FitSync.Gui", "start")
    .WithReference(api)
    .WithEnvironment("VITE_API_URL", api.GetEndpoint("http"))
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

builder.Build().Run();
