using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using IEMS.Core.Configuration;

namespace IEMS.Infrastructure.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlite(DatabaseLocation.ConnectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}