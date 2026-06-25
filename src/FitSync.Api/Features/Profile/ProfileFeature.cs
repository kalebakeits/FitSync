namespace FitSync.Api.Features.Profile;

using FitSync.Api.Features.Profile.Services;

public static class ProfileFeature
{
    public static IServiceCollection AddProfileFeature(this IServiceCollection services)
    {
        services.AddScoped<IProfileService, ProfileService>();
        return services;
    }
}
