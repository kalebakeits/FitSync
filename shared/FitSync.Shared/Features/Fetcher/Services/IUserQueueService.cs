namespace FitSync.Shared.Features.Fetcher.Services;

using FitSync.Database.Models;

public interface IUserQueuerService
{
    /// <summary>
    /// Gets the users that are for processing.
    /// </summary>
    /// <returns>An array of <see cref="User"/>.</returns>
    Task<User[]> GetDueUsersAsync();

    /// <summary>
    /// Release the users for future processing.
    /// </summary>
    /// <returns>A value indicating whether the users were successfully released.</returns>
    Task<bool> ReleaseUsersAsync(User[] users);
}
