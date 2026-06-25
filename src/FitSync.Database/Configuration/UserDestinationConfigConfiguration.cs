namespace FitSync.Database.Configuration;

using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserDestinationConfigConfiguration : IEntityTypeConfiguration<UserDestinationConfig>
{
    public void Configure(EntityTypeBuilder<UserDestinationConfig> entity)
    {
        entity.HasKey(
            e =>
                new
                {
                    e.UserId,
                    e.SourceServiceType,
                    e.DestinationServiceType
                }
        );

        entity
            .HasIndex(
                e =>
                    new
                    {
                        e.UserId,
                        e.SourceServiceType,
                        e.DestinationServiceType
                    }
            )
            .IsUnique();

        entity
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
