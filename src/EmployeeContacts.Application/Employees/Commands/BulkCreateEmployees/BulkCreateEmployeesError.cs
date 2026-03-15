namespace EmployeeContacts.Application.Employees.Commands.BulkCreateEmployees;

public sealed record BulkCreateEmployeesError(
    int Row,
    string Field,
    string Code,
    string Message);
