using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ContactManagement.Infrastructure.Data
{
    public class ContactDbContextFactory : IDesignTimeDbContextFactory<ContactDbContext>
    {
        public ContactDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ContactDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost,1433;Database=ContactDb;User Id=sa;Password=1a2b3c4d;TrustServerCertificate=True;");

            return new ContactDbContext(optionsBuilder.Options);
        }
    }
}
