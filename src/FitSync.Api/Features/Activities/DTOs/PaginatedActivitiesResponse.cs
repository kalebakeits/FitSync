namespace FitSync.Api.Features.Activities.DTOs;

public class PaginatedActivitiesResponse
{
    public required List<ActivityResponse> Items { get; init; }
    public required int Total { get; init; }
    public required int Limit { get; init; }
    public required int Offset { get; init; }
}
