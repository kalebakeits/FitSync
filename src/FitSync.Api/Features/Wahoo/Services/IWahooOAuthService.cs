namespace FitSync.Api.Features.Wahoo.Services;

public interface IWahooOAuthService
{
    Task<WahooTokenResult> ExchangeCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    );

    Task<WahooTokenResult> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default
    );

    Task<long> GetWahooUserIdAsync(string accessToken, CancellationToken cancellationToken = default);
}
