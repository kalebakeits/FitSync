namespace FitSync.Api.Features.Activities.DTOs;

public record PaginatedActivitiesResponse(
    List<ActivityResponse> Items,
    int Total,
    int Limit,
    int Offset
);
