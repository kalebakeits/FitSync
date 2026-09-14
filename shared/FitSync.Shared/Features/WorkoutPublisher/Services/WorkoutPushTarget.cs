namespace FitSync.Shared.Features.WorkoutPublisher.Services;

using FitSync.Database.Models;

public record WorkoutPushTarget(IWorkoutPublisherClient Client, Integration Integration);
