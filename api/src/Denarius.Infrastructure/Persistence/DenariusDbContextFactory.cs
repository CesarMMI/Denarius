using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Denarius.Infrastructure.Persistence;

public class DenariusDbContextFactory : IDesignTimeDbContextFactory<DenariusDbContext>
{
    public DenariusDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DenariusDbContext>()
            .UseSqlite("Data Source=denarius.db")
            .Options;

        return new DenariusDbContext(options);
    }
}
