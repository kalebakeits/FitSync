namespace FitSync.Api.Features.Workouts.DTOs;

public record UpdateWorkoutRequest(string Name, string? Description, List<string> Tags);
