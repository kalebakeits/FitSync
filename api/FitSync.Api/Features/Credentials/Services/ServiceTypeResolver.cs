namespace FitSync.Api.Features.Credentials.Services;

public class ServiceTypeResolver(
    ServiceCredentialHandlerFactory handlerFactory,
    IEnumerable<IOAuthServiceHandler> oauthHandlers
) : IServiceTypeResolver
{
    private readonly ServiceCredentialHandlerFactory handlerFactory = handlerFactory;
    private readonly Dictionary<string, IOAuthServiceHandler> oauthHandlerMap =
        oauthHandlers.ToDictionary(h => h.ServiceType, h => h);

    public bool IsFetcher(string serviceType)
    {
        IServiceCredentialHandler? credHandler = this.handlerFactory.Get(serviceType);
        if (credHandler != null)
            return credHandler.IsFetcher;

        this.oauthHandlerMap.TryGetValue(serviceType, out IOAuthServiceHandler? oauthHandler);
        return oauthHandler?.IsFetcher ?? false;
    }

    public bool IsUploader(string serviceType)
    {
        IServiceCredentialHandler? credHandler = this.handlerFactory.Get(serviceType);
        if (credHandler != null)
            return credHandler.IsUploader;

        this.oauthHandlerMap.TryGetValue(serviceType, out IOAuthServiceHandler? oauthHandler);
        return oauthHandler?.IsUploader ?? false;
    }
}
