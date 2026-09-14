namespace FitSync.Shared.Features.ActivityIngest.Services;

using FitSync.Shared.Features.ActivityIngest.Enums;

public class SportCategoryResolver : ISportCategoryResolver
{
    public SportCategory Resolve(int? sport) =>
        sport switch
        {
            5 or 85 => SportCategory.Swim,
            2 or 15 or 21 => SportCategory.Bike,
            1 or 11 or 17 => SportCategory.Run,
            _ => SportCategory.Other,
        };

    public string ToCategoryName(SportCategory category) =>
        category switch
        {
            SportCategory.Swim => "swim",
            SportCategory.Bike => "bike",
            SportCategory.Run => "run",
            _ => "other",
        };

    public bool IsValidCategoryName(string categoryName) =>
        categoryName is "swim" or "bike" or "run" or "other";
}
