namespace FitSync.Api.Features.Connections.Services;

using FitSync.Api.Exceptions;
using FitSync.Api.Features.Connections.DTOs;
using FitSync.Database;
using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;

public class DestinationMappingService(
    FitSyncDbContext context,
    ILogger<DestinationMappingService> logger
) : IDestinationMappingService
{
    private readonly FitSyncDbContext context = context;
    private readonly ILogger<DestinationMappingService> logger = logger;

    public async Task<List<DestinationMappingResponse>> GetMappingsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await this.context.UserDestinationConfigs.Where(c => c.UserId == userId)
            .GroupBy(c => c.SourceServiceType)
            .Select(
                g =>
                    new DestinationMappingResponse(
                        g.Key,
                        g.Select(c => c.DestinationServiceType).ToList()
                    )
            )
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertMappingsAsync(
        Guid userId,
        UpsertDestinationMappingsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        // UpsertDestinationMappingsRequest validates this, but the delete below executes
        // against the database immediately: a caller bypassing model validation would
        // otherwise lose every mapping for this source and get nothing back.
        if (request.DestinationServiceTypes.Contains(request.SourceServiceType))
            throw new BadRequestException(
                "Source and destination service types must be different."
            );

        await this.context.UserDestinationConfigs.Where(
            c => c.UserId == userId && c.SourceServiceType == request.SourceServiceType
        )
            .ExecuteDeleteAsync(cancellationToken);

        foreach (string dest in request.DestinationServiceTypes)
        {
            this.context.UserDestinationConfigs.Add(
                new UserDestinationConfig
                {
                    UserId = userId,
                    SourceServiceType = request.SourceServiceType,
                    DestinationServiceType = dest,
                }
            );
        }

        await this.context.SaveChangesAsync(cancellationToken);
        this.logger.LogInformation(
            "Upserted mappings for user {UserId}: {Source} -> [{Destinations}].",
            userId,
            request.SourceServiceType,
            string.Join(", ", request.DestinationServiceTypes)
        );
    }
}
