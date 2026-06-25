namespace FitSync.Database.Configuration;

using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ActivityUploadStatusConfiguration : IEntityTypeConfiguration<ActivityUploadStatus>
{
    public void Configure(EntityTypeBuilder<ActivityUploadStatus> entity)
    {
        entity.HasKey(e => new { e.ActivityId, e.DestinationServiceType });

        entity
            .HasOne(e => e.Activity)
            .WithMany(a => a.UploadStatuses)
            .HasForeignKey(e => e.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
