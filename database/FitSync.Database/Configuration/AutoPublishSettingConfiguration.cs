namespace FitSync.Database.Configuration;

using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AutoPublishSettingConfiguration : IEntityTypeConfiguration<AutoPublishSetting>
{
    public void Configure(EntityTypeBuilder<AutoPublishSetting> entity)
    {
        entity.HasIndex(e => new { e.UserId, e.ServiceType, e.SportCategory }).IsUnique();

        entity
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
