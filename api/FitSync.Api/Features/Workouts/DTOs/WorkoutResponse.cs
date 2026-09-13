namespace FitSync.Api.Features.Workouts.DTOs;

using System.Text.Json.Nodes;

public record WorkoutResponse(
    Guid Id,
    string Name,
    string? Description,
    List<string> Tags,
    int Sport,
    JsonNode? Schema,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
