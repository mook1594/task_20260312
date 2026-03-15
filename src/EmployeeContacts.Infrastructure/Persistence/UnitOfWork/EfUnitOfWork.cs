using EmployeeContacts.Application.Abstractions.Persistence;

namespace EmployeeContacts.Infrastructure.Persistence.UnitOfWork;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext dbContext;

    public EfUnitOfWork(AppDbContext dbContext)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
