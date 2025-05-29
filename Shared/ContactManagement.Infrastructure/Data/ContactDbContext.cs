using ContactManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContactManagement.Infrastructure.Data;

public class ContactDbContext : DbContext
{
    public DbSet<Contact> Contacts { get; set; } = default!;
    public ContactDbContext(DbContextOptions<ContactDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contact>().HasKey(c => c.Id);
        modelBuilder.Entity<Contact>().Property(c => c.Name).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<Contact>().Property(c => c.Email).IsRequired().HasMaxLength(255);
        modelBuilder.Entity<Contact>().Property(c => c.Phone).IsRequired().HasMaxLength(9);
        modelBuilder.Entity<Contact>().Property(c => c.Ddd).IsRequired().HasMaxLength(2);
        modelBuilder.Entity<Contact>().Property(c => c.LastReadAt).HasColumnType("datetimeoffset").IsRequired(false);
    }
}
