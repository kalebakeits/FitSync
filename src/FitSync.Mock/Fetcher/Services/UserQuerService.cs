namespace FitSync.Mock.Fetcher.Services;

using System.Text.Json;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Mock.Fetcher.Configuration;
using FitSync.Shared.Features.Fetcher.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

/// <summary>
/// A mock user queuer service that returns the default user from the database.
/// </summary>
/// <param name="fitSyncDbContext"></param>
/// <param name="logger"></param>
public class UserQueuerService(
    FitSyncDbContext fitSyncDbContext,
    ILogger<UserQueuerService> logger,
    IOptions<MockFetcherOptions> options
) : IUserQueuerService
{
    private readonly FitSyncDbContext fitSyncDbContext = fitSyncDbContext;
    private readonly ILogger<UserQueuerService> logger = logger;
    private readonly IOptions<MockFetcherOptions> options = options;

    /// <inheritdoc/>
    public async Task<User[]> GetDueUsersAsync()
    {
        this.logger.LogDebug("Fetching due users batch");
        User? user = await this.fitSyncDbContext.Users.FirstOrDefaultAsync(
            u => u.Username == "default"
        );
        this.logger.LogDebug("Found user {User}", JsonSerializer.Serialize(user));
        return user == null ? [] : [user];
    }

    public async Task<bool> ReleaseUsersAsync(User[] users)
    {
        TimeSpan sleepDuration = TimeSpan.FromMinutes(this.options.Value.PollIntervalMinutes);
        this.logger.LogDebug(
            "Releasing all users in {Users} and sleeping for {@SleepTime}",
            JsonSerializer.Serialize(users),
            sleepDuration
        );
        await Task.Delay(sleepDuration); // Fetcher always returns a user when running so we need to pause on release
        return true;
    }
}
