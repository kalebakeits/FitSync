namespace FitSync.Database.Configuration;

using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TrainingProfileConfiguration : IEntityTypeConfiguration<TrainingProfile>
{
    public void Configure(EntityTypeBuilder<TrainingProfile> entity)
    {
        entity
            .HasOne(e => e.User)
            .WithOne()
            .HasForeignKey<TrainingProfile>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => e.UserId).IsUnique();
    }
}
