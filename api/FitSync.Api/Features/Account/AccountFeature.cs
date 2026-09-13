namespace FitSync.Api.Features.Account;

using FitSync.Api.Features.Account.Services;

public static class AccountFeature
{
    public static IServiceCollection AddAccountFeature(this IServiceCollection services) =>
        services.AddScoped<IAccountService, AccountService>();
}
