namespace FitSync.Wahoo.Shared.WahooClient.Services;

using System.Net;
using FitSync.Database.Models;
using Microsoft.Extensions.Logging;

public class WahooHttpSender(
    HttpClient httpClient,
    IWahooAuthService authService,
    ILogger<WahooHttpSender> logger
) : IWahooHttpSender
{
    private readonly HttpClient httpClient = httpClient;
    private readonly IWahooAuthService authService = authService;
    private readonly ILogger<WahooHttpSender> logger = logger;

    public async Task<HttpResponseMessage> SendAsync(
        Integration integration,
        Func<HttpRequestMessage> buildRequest,
        CancellationToken cancellationToken = default
    )
    {
        await this.authService.EnsureAuthenticatedAsync(integration, cancellationToken);

        HttpResponseMessage response = await this.httpClient.SendAsync(
            buildRequest(),
            cancellationToken
        );

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            this.logger.LogWarning("Received 401 from Wahoo, retrying after token refresh.");
            await this.authService.EnsureAuthenticatedAsync(integration, cancellationToken);
            response = await this.httpClient.SendAsync(buildRequest(), cancellationToken);
        }

        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync(cancellationToken);
            this.logger.LogError(
                "Wahoo API returned {Status}: {Body}",
                (int)response.StatusCode,
                body
            );
        }

        response.EnsureSuccessStatusCode();

        return response;
    }
}
