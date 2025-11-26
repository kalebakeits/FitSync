namespace FitSync.MockFetcher.Services;

using System.Text.Json;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.Services;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// A mock user queuer service that returns the default user from the database.
/// </summary>
/// <param name="fitSyncDbContext"></param>
/// <param name="logger"></param>
public class UserQueuerService(FitSyncDbContext fitSyncDbContext, ILogger<UserQueuerService> logger)
    : IUserQueuerService
{
    private readonly FitSyncDbContext fitSyncDbContext = fitSyncDbContext;
    private readonly ILogger<UserQueuerService> logger = logger;

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

    public Task<bool> ReleaseUsersAsync(User[] users)
    {
        this.logger.LogDebug("Releasing all users in {Users}", JsonSerializer.Serialize(users));
        return Task.FromResult(true);
    }
}
