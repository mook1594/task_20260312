namespace EmployeeContacts.Application.Employees.Commands.BulkCreateEmployees;

public sealed record BulkEmployeeRecord(
    int Row,
    string Name,
    string Email,
    string Tel,
    string Joined);
