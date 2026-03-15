using EmployeeContacts.Infrastructure.Persistence.Configurations;
using EmployeeContacts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeContacts.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<EmployeeEntity> Employees => Set<EmployeeEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
    }
}
