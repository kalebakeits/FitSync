namespace FitSync.Garmin.Features.FitModification;

using FitSync.Garmin.Features.FitModification.Services;

public static class FitModificationFeatureExtensions
{
    public static IServiceCollection AddFitModification(this IServiceCollection services)
    {
        services.AddScoped<IFitModifier, FitBinaryPatcher>();

        return services;
    }
}
