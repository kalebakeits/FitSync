namespace FitSync.Api.Features.AutoPublish.DTOs;

public record AutoPublishSettingsRequest(List<AutoPublishSettingRequestItem> Settings);

public record AutoPublishSettingRequestItem(string ServiceType, string SportCategory);
