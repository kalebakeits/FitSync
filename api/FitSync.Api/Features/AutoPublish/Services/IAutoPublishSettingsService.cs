namespace FitSync.Api.Features.AutoPublish.Services;

using FitSync.Api.Features.AutoPublish.DTOs;

public interface IAutoPublishSettingsService
{
    Task<List<AutoPublishSettingResponse>> GetSettingsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task ReplaceSettingsAsync(
        Guid userId,
        AutoPublishSettingsRequest request,
        CancellationToken cancellationToken = default
    );
}
