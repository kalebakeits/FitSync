namespace FitSync.Database.Configuration;

using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> entity)
    {
        entity
            .HasOne(e => e.ScheduledWorkout)
            .WithMany()
            .HasForeignKey(e => e.ScheduledWorkoutId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
