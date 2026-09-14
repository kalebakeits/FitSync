namespace FitSync.Api.Features.Credentials.Services;

using FitSync.Database.Models;

public interface IOAuthServiceHandler
{
    string ServiceType { get; }
    Database.Enums.ServiceType? HeartbeatServiceType { get; }
    bool IsFetcher { get; }
    bool IsUploader { get; }
    bool SupportsWorkoutPublishing { get; }
    string AuthType { get; }
    string ConnectUrl { get; }
    string? GetDisplayName(Integration integration);
}
