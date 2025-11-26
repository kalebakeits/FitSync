namespace FitSync.MockFetcher.Services;

using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// Health check that reports unhealthy until database initialization is complete.
/// </summary>
public class DbInitializerHealthCheck : IHealthCheck
{
    private const string HealthyMessage = "Database initialization complete";
    private const string UnhealthyMessage = "Database initialization in progress";

    private bool isHealthy = false;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        if (this.isHealthy)
        {
            return Task.FromResult(HealthCheckResult.Healthy(HealthyMessage));
        }

        return Task.FromResult(HealthCheckResult.Unhealthy(UnhealthyMessage));
    }

    public void MarkAsHealthy()
    {
        this.isHealthy = true;
    }

    public void MarkAsUnhealthy()
    {
        this.isHealthy = false;
    }
}
