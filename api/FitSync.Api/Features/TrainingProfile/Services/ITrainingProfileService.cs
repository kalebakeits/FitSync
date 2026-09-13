namespace FitSync.Api.Features.TrainingProfile.Services;

using FitSync.Api.Features.TrainingProfile.DTOs;

public interface ITrainingProfileService
{
    Task<TrainingProfileResponse?> GetProfileAsync(Guid userId);
    Task<TrainingProfileResponse> UpsertProfileAsync(
        Guid userId,
        UpsertTrainingProfileRequest request
    );
}
