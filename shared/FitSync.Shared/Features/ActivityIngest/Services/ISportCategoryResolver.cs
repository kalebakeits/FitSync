namespace FitSync.Shared.Features.ActivityIngest.Services;

using FitSync.Shared.Features.ActivityIngest.Enums;

public interface ISportCategoryResolver
{
    SportCategory Resolve(int? sport);
    string ToCategoryName(SportCategory category);
    bool IsValidCategoryName(string categoryName);
}
