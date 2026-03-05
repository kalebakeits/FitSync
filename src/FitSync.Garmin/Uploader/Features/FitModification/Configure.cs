namespace FitSync.Garmin.Uploader.Features.FitModification;

using FitSync.Garmin.Uploader.Features.FitModification.Services;

public static class FitModificationFeatureExtensions
{
    public static IServiceCollection AddFitModification(this IServiceCollection services)
    {
        services.AddScoped<IFitFileDecoder, FitFileDecoder>();
        services.AddScoped<IFitFileEncoder, FitFileEncoder>();
        services.AddScoped<IDeviceInfoModifier, DeviceInfoModifier>();
        services.AddScoped<IFitModifier, FitModifier>();
        services.AddScoped<IWahooFitModifier, WahooFitModifier>();
        services.AddScoped<IFitModifierFactory, FitModifierFactory>();

        return services;
    }
}
