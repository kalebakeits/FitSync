namespace FitSync.Api.Features.Credentials.Services;

using FitSync.Database.Models;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using FitSync.Wahoo.Shared.AuthData;

public class WahooOAuthHandler(IEncryptionService encryptionService) : IOAuthServiceHandler
{
    private readonly IEncryptionService encryptionService = encryptionService;

    public string ServiceType => ServiceTypes.Wahoo;
    public string AuthType => "oauth";
    public string ConnectUrl => "/api/wahoo/connect";

    public string? GetDisplayName(Integration integration) =>
        integration.GetAuthData<WahooAuthData>(this.encryptionService).WahooUserId.ToString();
}
