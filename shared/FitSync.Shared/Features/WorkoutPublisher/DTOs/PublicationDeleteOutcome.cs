namespace FitSync.Shared.Features.WorkoutPublisher.DTOs;

public record PublicationDeleteOutcome(string ServiceType, bool Succeeded, string? Error);
