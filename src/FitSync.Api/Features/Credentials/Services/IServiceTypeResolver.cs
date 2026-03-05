namespace FitSync.Api.Features.Credentials.Services;

public interface IServiceTypeResolver
{
    bool IsFetcher(string serviceType);
    bool IsUploader(string serviceType);
}
