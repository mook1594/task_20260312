namespace EmployeeContacts.Application.Employees.Commands.BulkCreateEmployees;

public sealed record BulkCreateEmployeesResult(
    int Total,
    int Created,
    int Failed,
    IReadOnlyList<BulkCreateEmployeesError> Errors);
