namespace FitSync.Database.Configuration;

using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ScheduledWorkoutPublicationConfiguration
    : IEntityTypeConfiguration<ScheduledWorkoutPublication>
{
    public void Configure(EntityTypeBuilder<ScheduledWorkoutPublication> entity)
    {
        entity.HasIndex(e => new { e.ScheduledWorkoutId, e.ServiceType }).IsUnique();

        entity
            .HasOne(e => e.ScheduledWorkout)
            .WithMany(e => e.Publications)
            .HasForeignKey(e => e.ScheduledWorkoutId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
