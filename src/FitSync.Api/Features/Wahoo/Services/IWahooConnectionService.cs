namespace FitSync.Api.Features.Wahoo.Services;

public interface IWahooConnectionService
{
    string BuildAuthorizeUrl(Guid userId);

    Task CompleteAuthorizationAsync(
        string state,
        string code,
        CancellationToken cancellationToken = default
    );
}
