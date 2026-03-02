namespace FitSync.Api.Features.Credentials.Services;

using FitSync.Database.Models;

public interface IOAuthServiceHandler
{
    string ServiceType { get; }
    string AuthType { get; }
    string ConnectUrl { get; }
    string? GetDisplayName(Integration integration);
}
