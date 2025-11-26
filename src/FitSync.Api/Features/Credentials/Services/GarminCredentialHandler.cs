using FitSync.Database.Models;

namespace FitSync.Api.Features.Credentials.Services;

public class GarminCredentialHandler(ILogger<GarminCredentialHandler> logger)
    : IServiceCredentialHandler
{
    private readonly ILogger<GarminCredentialHandler> logger = logger;

    public string ServiceType => ServiceTypes.Garmin;

    public Task OnCredentialCreatedAsync(Guid userId)
    {
        this.logger.LogInformation(
            "Garmin credential created for user: {UserId} - no additional configuration needed",
            userId
        );
        return Task.CompletedTask;
    }

    public Task OnCredentialDeletedAsync(Guid userId)
    {
        this.logger.LogInformation(
            "Garmin credential deleted for user: {UserId} - no additional cleanup needed",
            userId
        );
        return Task.CompletedTask;
    }
}
