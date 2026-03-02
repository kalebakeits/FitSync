namespace FitSync.Api.Features.Connections;

using FitSync.Api.Features.Connections.Services;

public static class ConnectionsServiceExtensions
{
    public static IServiceCollection AddConnectionsFeature(this IServiceCollection services)
    {
        services.AddScoped<IConnectionsService, ConnectionsService>();
        return services;
    }
}
