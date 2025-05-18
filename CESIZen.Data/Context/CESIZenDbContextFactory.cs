using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CESIZen.Data.Context;

public class CESIZenDbContextFactory : IDesignTimeDbContextFactory<CESIZenDbContext>
{
    public CESIZenDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CESIZenDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=CESIZenDatabase;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new CESIZenDbContext(optionsBuilder.Options);
    }
}