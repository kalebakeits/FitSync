namespace FitSync.Api.Features.Connections.Services;

using FitSync.Api.Configurations;
using FitSync.Api.Features.Connections.DTOs;
using FitSync.Api.Features.Credentials.Services;
using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class FetcherStatusService(
    FitSyncDbContext context,
    IServiceTypeResolver serviceTypeResolver,
    ServiceCredentialHandlerFactory credentialHandlerFactory,
    IEnumerable<IOAuthServiceHandler> oauthHandlers,
    IOptions<AppConfiguration> appConfiguration
) : IFetcherStatusService
{
    private readonly FitSyncDbContext context = context;
    private readonly IServiceTypeResolver serviceTypeResolver = serviceTypeResolver;
    private readonly IOptions<AppConfiguration> appConfiguration = appConfiguration;

    // Map from Integration.ServiceType string → ServiceType enum for heartbeat lookups
    private readonly Dictionary<string, ServiceType> heartbeatServiceTypeMap = BuildHeartbeatMap(
        credentialHandlerFactory,
        oauthHandlers
    );

    private static Dictionary<string, ServiceType> BuildHeartbeatMap(
        ServiceCredentialHandlerFactory credentialHandlerFactory,
        IEnumerable<IOAuthServiceHandler> oauthHandlers
    )
    {
        Dictionary<string, ServiceType> map = [];

        foreach (IServiceCredentialHandler handler in credentialHandlerFactory.AllHandlers)
        {
            if (handler.HeartbeatServiceType.HasValue)
                map[handler.ServiceType] = handler.HeartbeatServiceType.Value;
        }

        foreach (IOAuthServiceHandler handler in oauthHandlers)
        {
            if (handler.HeartbeatServiceType.HasValue)
                map[handler.ServiceType] = handler.HeartbeatServiceType.Value;
        }

        return map;
    }

    public async Task<List<FetcherStatusResponse>> GetStatusAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        int maxFailures = this.appConfiguration.Value.MaxSequentialCredentialFailures;
        DateTime heartbeatCutoff = DateTime.UtcNow.AddMinutes(
            -this.appConfiguration.Value.FetcherHeartbeatThresholdMinutes
        );

        List<Integration> integrations = await this.context.Integrations
            .Where(i => i.UserId == userId)
            .ToListAsync(cancellationToken);

        List<UserDestinationConfig> mappings = await this.context.UserDestinationConfigs
            .Where(c => c.UserId == userId)
            .ToListAsync(cancellationToken);

        List<ServiceType> fetcherServiceTypes = integrations
            .Where(i => this.serviceTypeResolver.IsFetcher(i.ServiceType))
            .Where(i => this.heartbeatServiceTypeMap.ContainsKey(i.ServiceType))
            .Select(i => this.heartbeatServiceTypeMap[i.ServiceType])
            .ToList();

        HashSet<ServiceType> aliveServiceTypes = [
            ..await this.context.ServiceHeartbeats
                .Where(h => fetcherServiceTypes.Contains(h.ServiceType) && h.LastHeartbeatAt > heartbeatCutoff)
                .Select(h => h.ServiceType)
                .ToListAsync(cancellationToken)
        ];

        return integrations
            .Where(i => this.serviceTypeResolver.IsFetcher(i.ServiceType))
            .Select(fetcher => this.BuildStatus(fetcher, integrations, mappings, maxFailures, aliveServiceTypes))
            .ToList();
    }

    private FetcherStatusResponse BuildStatus(
        Integration fetcher,
        List<Integration> allIntegrations,
        List<UserDestinationConfig> mappings,
        int maxFailures,
        HashSet<ServiceType> aliveServiceTypes
    )
    {
        bool fetcherAlive = !this.heartbeatServiceTypeMap.TryGetValue(fetcher.ServiceType, out ServiceType heartbeatType)
            || aliveServiceTypes.Contains(heartbeatType);

        if (!fetcherAlive || fetcher.FailureCount >= maxFailures)
            return new FetcherStatusResponse
            {
                ServiceType = fetcher.ServiceType,
                Status = "red",
                Reason = FetcherStatusReason.FetcherUnhealthy,
                Destinations = [],
            };

        List<string> mappedDests = mappings
            .Where(m => m.SourceServiceType == fetcher.ServiceType)
            .Select(m => m.DestinationServiceType)
            .ToList();

        if (mappedDests.Count == 0)
            return new FetcherStatusResponse
            {
                ServiceType = fetcher.ServiceType,
                Status = "grey",
                Reason = FetcherStatusReason.NoDestinations,
                Destinations = [],
            };

        List<DestinationStatusEntry> destinations = mappedDests
            .Select(dest => this.BuildDestinationEntry(dest, allIntegrations, maxFailures))
            .ToList();

        if (destinations.All(d => d.Healthy))
            return new FetcherStatusResponse
            {
                ServiceType = fetcher.ServiceType,
                Status = "green",
                Reason = FetcherStatusReason.None,
                Destinations = destinations,
            };

        if (destinations.All(d => !d.Healthy))
            return new FetcherStatusResponse
            {
                ServiceType = fetcher.ServiceType,
                Status = "red",
                Reason = FetcherStatusReason.AllDestinationsUnhealthy,
                Destinations = destinations,
            };

        return new FetcherStatusResponse
        {
            ServiceType = fetcher.ServiceType,
            Status = "amber",
            Reason = FetcherStatusReason.SomeDestinationsUnhealthy,
            Destinations = destinations,
        };
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
