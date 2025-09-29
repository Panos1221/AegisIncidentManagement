using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IncidentManagement.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IncidentManagementDbContext>
{
    public IncidentManagementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IncidentManagementDbContext>();
        
        // Use a default connection string for design-time operations
        var connectionString = "Server=(localdb)\\mssqllocaldb;Database=IncidentManagementDb;Trusted_Connection=true;MultipleActiveResultSets=true";
        
        optionsBuilder.UseSqlServer(connectionString);
        
        return new IncidentManagementDbContext(optionsBuilder.Options);
    }
}