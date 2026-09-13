namespace FitSync.Purger.Features.ActivityPurger.Services;

public interface IActivityPurgerService
{
    Task PurgeAsync(CancellationToken ct);
}
