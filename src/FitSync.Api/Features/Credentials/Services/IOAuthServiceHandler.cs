namespace FitSync.Api.Features.Credentials.Services;

using FitSync.Database.Models;

public interface IOAuthServiceHandler
{
    string ServiceType { get; }
    bool IsFetcher { get; }
    bool IsUploader { get; }
    string AuthType { get; }
    string ConnectUrl { get; }
    string? GetDisplayName(Integration integration);
}
