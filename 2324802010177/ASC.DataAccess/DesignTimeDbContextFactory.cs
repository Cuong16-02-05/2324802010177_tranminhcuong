using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ASC.DataAccess
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\MSSQLLocalDB;Database=ASC;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True",
                b => b.MigrationsAssembly("ASC.DataAccess"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
