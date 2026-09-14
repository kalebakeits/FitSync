namespace FitSync.Api.Features.AutoPublish.Services;

using FitSync.Api.Exceptions;
using FitSync.Api.Features.AutoPublish.DTOs;
using FitSync.Api.Features.Credentials.Services;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.ActivityIngest.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public class AutoPublishSettingsService(
    FitSyncDbContext context,
    ServiceCredentialHandlerFactory handlerFactory,
    IEnumerable<IOAuthServiceHandler> oauthHandlers,
    ISportCategoryResolver sportCategoryResolver,
    ILogger<AutoPublishSettingsService> logger
) : IAutoPublishSettingsService
{
    private readonly FitSyncDbContext context = context;
    private readonly ServiceCredentialHandlerFactory handlerFactory = handlerFactory;
    private readonly IEnumerable<IOAuthServiceHandler> oauthHandlers = oauthHandlers;
    private readonly ISportCategoryResolver sportCategoryResolver = sportCategoryResolver;
    private readonly ILogger<AutoPublishSettingsService> logger = logger;

    public async Task<List<AutoPublishSettingResponse>> GetSettingsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation("Getting auto-publish settings for user {UserId}.", userId);

        List<AutoPublishSettingResponse> settings = await this.context.AutoPublishSettings.Where(
            s => s.UserId == userId
        )
            .OrderBy(s => s.ServiceType)
            .ThenBy(s => s.SportCategory)
            .Select(s => new AutoPublishSettingResponse(s.Id, s.ServiceType, s.SportCategory))
            .ToListAsync(cancellationToken);

        this.logger.LogInformation(
            "Retrieved {Count} auto-publish settings for user {UserId}.",
            settings.Count,
            userId
        );
        return settings;
    }

    public async Task ReplaceSettingsAsync(
        Guid userId,
        AutoPublishSettingsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "Replacing {Count} auto-publish settings for user {UserId}.",
            request.Settings.Count,
            userId
        );

        List<string> knownServiceTypes =
        [
            .. this.handlerFactory.ServiceTypes,
            .. this.oauthHandlers.Select(h => h.ServiceType),
        ];

        List<AutoPublishSettingRequestItem> distinctSettings =
        [
            .. request.Settings.DistinctBy(s => new { s.ServiceType, s.SportCategory }),
        ];

        if (distinctSettings.Count != request.Settings.Count)
            throw new BadRequestException("Duplicate service type and sport category entries.");

        foreach (AutoPublishSettingRequestItem item in distinctSettings)
        {
            if (!knownServiceTypes.Contains(item.ServiceType))
                throw new BadRequestException($"Unknown service type '{item.ServiceType}'.");

            if (!this.sportCategoryResolver.IsValidCategoryName(item.SportCategory))
                throw new BadRequestException($"Unknown sport category '{item.SportCategory}'.");
        }

        // The delete executes immediately, so validation has to happen before the transaction
        // opens: a rejected request must never leave the user with no settings at all.
        await using IDbContextTransaction transaction = await this.context.Database.BeginTransactionAsync(
            cancellationToken
        );

        int deleted = await this.context.AutoPublishSettings.Where(s => s.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        foreach (AutoPublishSettingRequestItem item in distinctSettings)
        {
            this.context.AutoPublishSettings.Add(
                new AutoPublishSetting
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ServiceType = item.ServiceType,
                    SportCategory = item.SportCategory,
                }
            );
        }

        await this.context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        this.logger.LogInformation(
            "Replaced auto-publish settings for user {UserId}: {Deleted} removed, {Inserted} inserted.",
            userId,
            deleted,
            distinctSettings.Count
        );
    }
}
