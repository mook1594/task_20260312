using EmployeeContacts.Application.Employees.Commands.BulkCreateEmployees;

namespace EmployeeContacts.Application.Abstractions.Parsing;

public interface IEmployeeImportParser
{
    Task<IReadOnlyList<BulkEmployeeRecord>> ParseAsync(string content, CancellationToken cancellationToken);
}
