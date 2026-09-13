namespace FitSync.Shared.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

public static class SerilogExtensions
{
    /// <summary>
    /// Adds Serilog to the application with a structured logging template.
    /// </summary>
    public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog(
            (context, services, configuration) =>
                ConfigureSerilog(
                    configuration,
                    context.HostingEnvironment.ApplicationName,
                    context.HostingEnvironment.EnvironmentName
                )
        );

        return builder;
    }

    /// <summary>
    /// Adds Serilog to the application with a structured logging template.
    /// </summary>
    public static HostApplicationBuilder AddSerilog(this HostApplicationBuilder builder)
    {
        builder.Services.AddSerilog(
            (services, configuration) =>
                ConfigureSerilog(
                    configuration,
                    builder.Environment.ApplicationName,
                    builder.Environment.EnvironmentName
                )
        );

        return builder;
    }

    private static void ConfigureSerilog(
        LoggerConfiguration configuration,
        string applicationName,
        string environmentName
    )
    {
        configuration
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", applicationName)
            .Enrich.WithProperty("Environment", environmentName)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}][{SourceContext}] {Message:lj}{NewLine}{Exception}"
            );
    }
}
