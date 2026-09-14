namespace FitSync.Garmin.Shared.GarminClient.Services;

using FitSync.Database.Models;
using Flurl.Http;

public interface IGarminHttpSender
{
    Task<IFlurlResponse> GetAsync(
        Integration integration,
        Func<string, IFlurlRequest> buildRequest,
        CancellationToken cancellationToken = default
    );
}
