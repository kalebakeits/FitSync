namespace FitSync.Api.Features.Credentials.Services;

public class ServiceCredentialHandlerFactory(
    IEnumerable<IServiceCredentialHandler> handlers,
    ILogger<ServiceCredentialHandlerFactory> logger
)
{
    private readonly Dictionary<string, IServiceCredentialHandler> handlerMap =
        handlers.ToDictionary(h => h.ServiceType, h => h);
    private readonly ILogger<ServiceCredentialHandlerFactory> logger = logger;

    public IServiceCredentialHandler? GetHandler(string serviceType)
    {
        if (this.handlerMap.TryGetValue(serviceType, out IServiceCredentialHandler? handler))
        {
            return handler;
        }

        this.logger.LogWarning("No handler found for service type: {ServiceType}", serviceType);
        return null;
    }
}
