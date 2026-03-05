namespace FitSync.Api.Features.Connections.Services;

using FitSync.Api.Configurations;
using FitSync.Api.Features.Connections.DTOs;
using FitSync.Api.Features.Credentials.Services;
using FitSync.Database;
using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class FetcherStatusService(
    FitSyncDbContext context,
    IServiceTypeResolver serviceTypeResolver,
    IOptions<AppConfiguration> appConfiguration
) : IFetcherStatusService
{
    private readonly FitSyncDbContext context = context;
    private readonly IServiceTypeResolver serviceTypeResolver = serviceTypeResolver;
    private readonly IOptions<AppConfiguration> appConfiguration = appConfiguration;

    public async Task<List<FetcherStatusResponse>> GetStatusAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        int maxFailures = this.appConfiguration.Value.MaxSequentialCredentialFailures;

        List<Integration> integrations = await this.context.Integrations
            .Where(i => i.UserId == userId)
            .ToListAsync(cancellationToken);

        List<UserDestinationConfig> mappings = await this.context.UserDestinationConfigs
            .Where(c => c.UserId == userId)
            .ToListAsync(cancellationToken);

        return integrations
            .Where(i => this.serviceTypeResolver.IsFetcher(i.ServiceType))
            .Select(fetcher => this.BuildStatus(fetcher, integrations, mappings, maxFailures))
            .ToList();
    }

    private FetcherStatusResponse BuildStatus(
        Integration fetcher,
        List<Integration> allIntegrations,
        List<UserDestinationConfig> mappings,
        int maxFailures
    )
    {
        List<string> mappedDests = mappings
            .Where(m => m.SourceServiceType == fetcher.ServiceType)
            .Select(m => m.DestinationServiceType)
            .ToList();

        if (mappedDests.Count == 0)
            return new FetcherStatusResponse { ServiceType = fetcher.ServiceType, Status = "grey", Destinations = [] };

        List<DestinationStatusEntry> destinations = mappedDests
            .Select(dest => this.BuildDestinationEntry(dest, allIntegrations, maxFailures))
            .ToList();

        string status = destinations.All(d => d.Healthy) ? "green"
            : destinations.All(d => !d.Healthy) ? "red"
            : "amber";

        return new FetcherStatusResponse { ServiceType = fetcher.ServiceType, Status = status, Destinations = destinations };
    }

    private DestinationStatusEntry BuildDestinationEntry(
        string destServiceType,
        List<Integration> allIntegrations,
        int maxFailures
    )
    {
        Integration? dest = allIntegrations.FirstOrDefault(i => i.ServiceType == destServiceType);

        return new DestinationStatusEntry
        {
            ServiceType = destServiceType,
            Connected = dest != null,
            Healthy = dest != null && dest.FailureCount < maxFailures,
        };
    }
}
