using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FitSync.Database;

public class FitSyncDbContext(DbContextOptions<FitSyncDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Session> Sessions { get; set; } = null!;
    public DbSet<Activity> Activities { get; set; } = null!;
    public DbSet<ServiceHeartbeat> ServiceHeartbeats { get; set; } = null!;
    public DbSet<ProcessedActivity> ProcessedActivities { get; set; } = null!;
    public DbSet<ZwiftFetcherConfig> ZwiftFetcherConfigs { get; set; } = null!;
    public DbSet<UserCredential> UserCredentials { get; set; } = null!;

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is User user)
            {
                if (entry.State == EntityState.Added)
                    user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Activity activity)
            {
                if (entry.State == EntityState.Added)
                    activity.CreatedAt = DateTime.UtcNow;
                activity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is ServiceHeartbeat heartbeat)
            {
                if (entry.State == EntityState.Added)
                    heartbeat.CreatedAt = DateTime.UtcNow;
                heartbeat.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is ZwiftFetcherConfig zwiftFetcherConfig)
            {
                if (entry.State == EntityState.Added)
                    zwiftFetcherConfig.CreatedAt = DateTime.UtcNow;
                zwiftFetcherConfig.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is UserCredential userCredential)
            {
                if (entry.State == EntityState.Added)
                    userCredential.CreatedAt = DateTime.UtcNow;
                userCredential.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Session session)
            {
                if (entry.State == EntityState.Added)
                    session.CreatedAt = DateTime.UtcNow;
            }
        }
    }
}
