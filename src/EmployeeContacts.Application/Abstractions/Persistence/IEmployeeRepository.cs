using EmployeeContacts.Application.Common.Models;
using EmployeeContacts.Application.Employees.Dtos;
using EmployeeContacts.Domain.Employees;

namespace EmployeeContacts.Application.Abstractions.Persistence;

public interface IEmployeeRepository
{
    Task<PagedResult<EmployeeDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);

    Task<IReadOnlyList<EmployeeDto>> GetByNameAsync(string exactName, CancellationToken cancellationToken);

    Task<IReadOnlySet<string>> GetExistingEmailsAsync(
        IReadOnlyCollection<string> emails,
        CancellationToken cancellationToken);

    Task<IReadOnlySet<string>> GetExistingPhoneNumbersAsync(
        IReadOnlyCollection<string> phoneNumbers,
        CancellationToken cancellationToken);

    Task AddRangeAsync(IReadOnlyCollection<Employee> employees, CancellationToken cancellationToken);
}
