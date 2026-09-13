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
    public DbSet<Workout> Workouts { get; set; } = null!;
    public DbSet<ScheduledWorkout> ScheduledWorkouts { get; set; } = null!;
    public DbSet<TrainingProfile> TrainingProfiles { get; set; } = null!;
    public DbSet<ApiToken> ApiTokens { get; set; } = null!;
    public DbSet<OAuthClient> OAuthClients { get; set; } = null!;
    public DbSet<OAuthCode> OAuthCodes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FitSyncDbContext).Assembly);
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
            else if (entry.Entity is Workout workout)
            {
                if (entry.State == EntityState.Added)
                    workout.CreatedAt = DateTime.UtcNow;
                workout.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is TrainingProfile profile)
            {
                if (entry.State == EntityState.Added)
                    profile.CreatedAt = DateTime.UtcNow;
                profile.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Session session)
            {
                if (entry.State == EntityState.Added)
                    session.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is ApiToken apiToken)
            {
                if (entry.State == EntityState.Added)
                    apiToken.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is OAuthClient oauthClient)
            {
                if (entry.State == EntityState.Added)
                    oauthClient.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is OAuthCode oauthCode)
            {
                if (entry.State == EntityState.Added)
                    oauthCode.CreatedAt = DateTime.UtcNow;
            }
        }
    }
}
