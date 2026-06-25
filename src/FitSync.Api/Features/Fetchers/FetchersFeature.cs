namespace FitSync.Api.Features.Fetchers;

using FitSync.Api.Features.Fetchers.Services;

public static class FetchersFeature
{
    public static IServiceCollection AddFetchersFeature(this IServiceCollection services)
    {
        services.AddScoped<IFetchersService, FetchersService>();
        return services;
    }
}
