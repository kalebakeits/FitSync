namespace FitSync.Database;

using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;

public class FitSyncDbContext(DbContextOptions<FitSyncDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Session> Sessions { get; set; } = null!;
    public DbSet<Activity> Activities { get; set; } = null!;
    public DbSet<ActivityUploadStatus> ActivityUploadStatuses { get; set; } = null!;
    public DbSet<ServiceHeartbeat> ServiceHeartbeats { get; set; } = null!;
    public DbSet<ProcessedActivity> ProcessedActivities { get; set; } = null!;
    public DbSet<Integration> Integrations { get; set; } = null!;
    public DbSet<FetcherConfig> FetcherConfigs { get; set; } = null!;
    public DbSet<UserDestinationConfig> UserDestinationConfigs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserDestinationConfig>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.SourceServiceType, e.DestinationServiceType });

            entity.HasIndex(e => new { e.UserId, e.SourceServiceType, e.DestinationServiceType })
                  .IsUnique();

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ActivityUploadStatus>(entity =>
        {
            entity.HasKey(e => new { e.ActivityId, e.DestinationServiceType });

            entity.HasOne(e => e.Activity)
                  .WithMany(a => a.UploadStatuses)
                  .HasForeignKey(e => e.ActivityId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

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
            else if (entry.Entity is Integration integration)
            {
                if (entry.State == EntityState.Added)
                    integration.CreatedAt = DateTime.UtcNow;
                integration.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is FetcherConfig fetcherConfig)
            {
                if (entry.State == EntityState.Added)
                    fetcherConfig.CreatedAt = DateTime.UtcNow;
                fetcherConfig.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is ActivityUploadStatus uploadStatus)
            {
                uploadStatus.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Session session)
            {
                if (entry.State == EntityState.Added)
                    session.CreatedAt = DateTime.UtcNow;
            }
        }
    }
}
