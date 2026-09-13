namespace FitSync.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class DesignTimeFitSyncDbContextFactory : IDesignTimeDbContextFactory<FitSyncDbContext>
{
    public FitSyncDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FitSyncDbContext>();

        // Use your local dev connection string here
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=FitSync;Username=postgres;Password=postgres"
        );

        return new FitSyncDbContext(optionsBuilder.Options);
    }
}
