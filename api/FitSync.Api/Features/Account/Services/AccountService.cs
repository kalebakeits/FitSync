namespace FitSync.Api.Features.Account.Services;

using FitSync.Database;
using Microsoft.EntityFrameworkCore;

public class AccountService(ILogger<AccountService> logger, FitSyncDbContext fitSyncDbContext)
    : IAccountService
{
    private readonly ILogger<AccountService> logger = logger;
    private readonly FitSyncDbContext fitSyncDbContext = fitSyncDbContext;

    public async Task DeleteUserAsync(Guid userId)
    {
        this.logger.LogInformation("Deleting user '{UserId}' and all associated data", userId);

        await this.fitSyncDbContext.Users.Where(a => a.Id == userId).ExecuteDeleteAsync();

        this.logger.LogInformation("User '{UserId}' successfully deleted", userId);
    }
}
