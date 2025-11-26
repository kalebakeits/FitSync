namespace FitSync.Api.Features.Account.Services;

public interface IAccountService
{
    Task DeleteUserAsync(Guid userId);
}
