using FitSync.Uploader.Features.FitModification.Services;

namespace FitSync.Uploader.Features.FitModification;

public static class FitModificationFeatureExtensions
{
    public static IServiceCollection AddFitModification(this IServiceCollection services)
    {
        services.AddScoped<IFitFileDecoder, FitFileDecoder>();
        services.AddScoped<IFitFileEncoder, FitFileEncoder>();
        services.AddScoped<IDeviceInfoModifier, DeviceInfoModifier>();
        services.AddScoped<IFitModifier, FitModifier>();

        return services;
    }
}
