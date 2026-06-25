namespace FitSync.Api.Features.Wahoo.Services;

using System.Net.Http.Headers;
using FitSync.Api.Configurations;
using FitSync.Api.Features.Wahoo.DTOs;
using FitSync.Wahoo.Shared.WahooClient.DTOs;
using Microsoft.Extensions.Options;

public class WahooAuthService(
    HttpClient httpClient,
    IOptions<WahooOptions> options,
    ILogger<WahooAuthService> logger
) : IWahooOAuthService
{
    private readonly HttpClient httpClient = httpClient;
    private readonly IOptions<WahooOptions> options = options;
    private readonly ILogger<WahooAuthService> logger = logger;

    public async Task<WahooTokenResult> ExchangeCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        string tokenUrl = $"{this.options.Value.BaseUrl.TrimEnd('/')}/oauth/token";
        FormUrlEncodedContent content =
            new(
                new Dictionary<string, string>
                {
                    ["client_id"] = this.options.Value.ClientId,
                    ["client_secret"] = this.options.Value.ClientSecret,
                    ["redirect_uri"] = this.options.Value.RedirectUri,
                    ["grant_type"] = "authorization_code",
                    ["code"] = code
                }
            );
        HttpResponseMessage response = await this.httpClient.PostAsync(
            tokenUrl,
            content,
            cancellationToken
        );
        response.EnsureSuccessStatusCode();
        WahooTokenRefreshResponse token =
            await response.Content.ReadFromJsonAsync<WahooTokenRefreshResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Wahoo token response could not be parsed.");
        this.logger.LogInformation("Wahoo OAuth code exchange completed.");
        return new WahooTokenResult
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddSeconds(token.ExpiresIn)
        };
    }

    public async Task<WahooTokenResult> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default
    )
    {
        string tokenUrl = $"{this.options.Value.BaseUrl.TrimEnd('/')}/oauth/token";
        FormUrlEncodedContent content =
            new(
                new Dictionary<string, string>
                {
                    ["client_id"] = this.options.Value.ClientId,
                    ["client_secret"] = this.options.Value.ClientSecret,
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = refreshToken
                }
            );
        HttpResponseMessage response = await this.httpClient.PostAsync(
            tokenUrl,
            content,
            cancellationToken
        );
        response.EnsureSuccessStatusCode();
        WahooTokenRefreshResponse token =
            await response.Content.ReadFromJsonAsync<WahooTokenRefreshResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Wahoo refresh response could not be parsed.");
        this.logger.LogInformation("Wahoo token refresh completed.");
        return new WahooTokenResult
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddSeconds(token.ExpiresIn)
        };
    }

    public async Task<long> GetWahooUserIdAsync(
        string accessToken,
        CancellationToken cancellationToken = default
    )
    {
        string userUrl = $"{this.options.Value.BaseUrl.TrimEnd('/')}/v1/user";
        HttpRequestMessage request = new(HttpMethod.Get, userUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        HttpResponseMessage response = await this.httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        WahooUserResponse user =
            await response.Content.ReadFromJsonAsync<WahooUserResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Wahoo user response could not be parsed.");
        this.logger.LogInformation("Fetched Wahoo user id from /v1/user.");
        return user.Id;
    }
}
