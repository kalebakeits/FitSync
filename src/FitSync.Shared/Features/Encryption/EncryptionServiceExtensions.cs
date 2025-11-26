namespace FitSync.Shared.Features.Encryption;

using FitSync.Shared.Configuration;
using FitSync.Shared.Features.Encryption.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class EncryptionServiceExtensions
{
    public static IServiceCollection AddEncryptionService(
        this IServiceCollection services,
        Func<IConfigurationSection> getConfigSection
    )
    {
        services
            .AddOptions<DataProtectionOptions>()
            .Bind(getConfigSection())
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IEncryptionService, EncryptionService>();

        return services;
    }
}
