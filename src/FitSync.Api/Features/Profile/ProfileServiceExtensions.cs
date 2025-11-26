using FitSync.Api.Features.Profile.Services;

namespace FitSync.Api.Features.Profile;

public static class ProfileServiceExtensions
{
    public static IServiceCollection AddProfileFeature(this IServiceCollection services)
    {
        services.AddScoped<IProfileService, ProfileService>();
        return services;
    }
}
