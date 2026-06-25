namespace FitSync.Wahoo.Shared.WahooClient.Services;

using FitSync.Database.Models;

public interface IWahooHttpSender
{
    Task<HttpResponseMessage> SendAsync(
        Integration integration,
        Func<HttpRequestMessage> buildRequest,
        CancellationToken cancellationToken = default
    );
}
