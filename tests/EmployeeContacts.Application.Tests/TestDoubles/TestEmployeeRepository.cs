using EmployeeContacts.Application.Abstractions.Persistence;
using EmployeeContacts.Application.Common.Models;
using EmployeeContacts.Application.Employees.Dtos;
using EmployeeContacts.Domain.Employees;

namespace EmployeeContacts.Application.Tests.TestDoubles;

internal sealed class TestEmployeeRepository : IEmployeeRepository
{
    public PagedResult<EmployeeDto> PagedResult { get; set; }
        = new([], 1, 20, 0, 0);

    public IReadOnlyList<EmployeeDto> EmployeesByNameResult { get; set; } = [];

    public HashSet<string> ExistingEmails { get; } = new(StringComparer.Ordinal);

    public HashSet<string> ExistingPhoneNumbers { get; } = new(StringComparer.Ordinal);

    public List<Employee> AddedEmployees { get; } = [];

    public int GetPagedCallCount { get; private set; }

    public int GetByNameCallCount { get; private set; }

    public int GetExistingEmailsCallCount { get; private set; }

    public int GetExistingPhoneNumbersCallCount { get; private set; }

    public int AddRangeCallCount { get; private set; }

    public int LastRequestedPage { get; private set; }

    public int LastRequestedPageSize { get; private set; }

    public string? LastRequestedName { get; private set; }

    public IReadOnlyCollection<string> LastEmailLookupValues { get; private set; } = [];

    public IReadOnlyCollection<string> LastPhoneLookupValues { get; private set; } = [];

    public Task<PagedResult<EmployeeDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        GetPagedCallCount++;
        LastRequestedPage = page;
        LastRequestedPageSize = pageSize;

        return Task.FromResult(PagedResult);
    }

    public Task<IReadOnlyList<EmployeeDto>> GetByNameAsync(string exactName, CancellationToken cancellationToken)
    {
        GetByNameCallCount++;
        LastRequestedName = exactName;

        return Task.FromResult(EmployeesByNameResult);
    }

    public Task<IReadOnlySet<string>> GetExistingEmailsAsync(
        IReadOnlyCollection<string> emails,
        CancellationToken cancellationToken)
    {
        GetExistingEmailsCallCount++;
        LastEmailLookupValues = emails.ToArray();

        return Task.FromResult<IReadOnlySet<string>>(new HashSet<string>(ExistingEmails, StringComparer.Ordinal));
    }

    public Task<IReadOnlySet<string>> GetExistingPhoneNumbersAsync(
        IReadOnlyCollection<string> phoneNumbers,
        CancellationToken cancellationToken)
    {
        GetExistingPhoneNumbersCallCount++;
        LastPhoneLookupValues = phoneNumbers.ToArray();

        return Task.FromResult<IReadOnlySet<string>>(new HashSet<string>(ExistingPhoneNumbers, StringComparer.Ordinal));
    }

    public Task AddRangeAsync(IReadOnlyCollection<Employee> employees, CancellationToken cancellationToken)
    {
        AddRangeCallCount++;
        AddedEmployees.AddRange(employees);

        return Task.CompletedTask;
    }
}
