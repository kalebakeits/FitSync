namespace FitSync.Garmin.Shared.GarminClient.Services;

using System.Net;
using FitSync.Database.Models;
using FitSync.Garmin.Shared.AuthData;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using Flurl.Http;
using Microsoft.Extensions.Logging;

public class GarminHttpSender(
    IGarminAuthService authService,
    IEncryptionService encryptionService,
    ILogger<GarminHttpSender> logger
) : IGarminHttpSender
{
    private readonly IGarminAuthService authService = authService;
    private readonly IEncryptionService encryptionService = encryptionService;
    private readonly ILogger<GarminHttpSender> logger = logger;

    public async Task<IFlurlResponse> GetAsync(
        Integration integration,
        Func<string, IFlurlRequest> buildRequest,
        CancellationToken cancellationToken = default
    )
    {
        await this.authService.EnsureAuthenticatedAsync(integration, cancellationToken);

        string accessToken = integration
            .GetAuthData<GarminAuthData>(this.encryptionService)
            .OAuth2AccessToken!;

        IFlurlResponse response = await buildRequest(accessToken)
            .AllowAnyHttpStatus()
            .GetAsync(cancellationToken: cancellationToken);

        if (response.StatusCode != Convert.ToInt32(HttpStatusCode.Unauthorized))
            return response;

        this.logger.LogWarning(
            "Garmin returned 401 for user {UserId}, retrying after token refresh.",
            integration.UserId
        );

        if (!await this.authService.TryRefreshAsync(integration, cancellationToken))
            return response;

        string refreshedToken = integration
            .GetAuthData<GarminAuthData>(this.encryptionService)
            .OAuth2AccessToken!;

        return await buildRequest(refreshedToken)
            .AllowAnyHttpStatus()
            .GetAsync(cancellationToken: cancellationToken);
    }
}
