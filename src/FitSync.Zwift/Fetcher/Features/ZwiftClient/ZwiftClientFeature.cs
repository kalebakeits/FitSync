namespace FitSync.Zwift.Fetcher.Features.ZwiftClient;

using FitSync.Zwift.Fetcher.Features.ZwiftClient.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Configures the necessary services for interacting with the Zwift API.
/// </summary>
public static class ZwiftClientFeature
{
    /// <summary>
    /// Adds the IZwiftClient interface and its concrete ZwiftClient implementation to the service collection.
    /// </summary>
    /// <param name="services">An instance of <see cref="IServiceCollection"/>.</param>
    /// <returns>The same instance of <see cref="IServiceCollection"/> for further chaining.</returns>
    public static IServiceCollection AddZwiftClient(this IServiceCollection services)
    {
        services.AddHttpClient<IZwiftAuthService, ZwiftAuthService>();
        services.AddHttpClient<IZwiftApiService, ZwiftApiService>();
        services.AddScoped<IZwiftActivityProcessor, ZwiftActivityProcessor>();
        services.AddScoped<IZwiftClient, ZwiftClient>();
        return services;
    }
}
