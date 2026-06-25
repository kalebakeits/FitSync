namespace FitSync.Api.Features.Fetchers.Services;

public interface IFetchersService
{
    Task TriggerFetchAsync(Guid userId);
}
