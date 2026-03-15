using EmployeeContacts.Application.Abstractions.Persistence;

namespace EmployeeContacts.Application.Tests.TestDoubles;

internal sealed class RecordingUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCallCount++;
        return Task.CompletedTask;
    }
}
