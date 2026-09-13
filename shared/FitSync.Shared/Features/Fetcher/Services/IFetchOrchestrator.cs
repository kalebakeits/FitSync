namespace FitSync.Shared.Features.Fetcher.Services;

using FitSync.Database.Models;

public interface IFetchOrchestrator
{
    Task ProcessUsersAsync(User[] users, CancellationToken cancellationToken = default);
}
