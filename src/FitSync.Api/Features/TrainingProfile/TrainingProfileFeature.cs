namespace FitSync.Api.Features.TrainingProfile;

using FitSync.Api.Features.TrainingProfile.Services;

public static class TrainingProfileFeature
{
    public static IServiceCollection AddTrainingProfileFeature(this IServiceCollection services)
    {
        services.AddScoped<ITrainingProfileService, TrainingProfileService>();
        return services;
    }
}
