namespace FitSync.Api.Features.Credentials.Services;

public class ServiceCredentialHandlerFactory(
    IEnumerable<IServiceCredentialHandler> handlers,
    ILogger<ServiceCredentialHandlerFactory> logger
)
{
    private readonly Dictionary<string, IServiceCredentialHandler> handlerMap =
        handlers.ToDictionary(h => h.ServiceType, h => h);
    private readonly ILogger<ServiceCredentialHandlerFactory> logger = logger;

    public List<string> ServiceTypes => [.. this.handlerMap.Keys];
    public IEnumerable<IServiceCredentialHandler> AllHandlers => this.handlerMap.Values;

    public IServiceCredentialHandler Require(string serviceType)
    {
        if (this.handlerMap.TryGetValue(serviceType, out IServiceCredentialHandler? handler))
            return handler;

        throw new InvalidOperationException(
            $"No credential handler registered for service type: {serviceType}"
        );
    }

    public IServiceCredentialHandler? Get(string serviceType)
    {
        if (this.handlerMap.TryGetValue(serviceType, out IServiceCredentialHandler? handler))
            return handler;

        this.logger.LogWarning("No handler found for service type: {ServiceType}", serviceType);
        return null;
    }
}
