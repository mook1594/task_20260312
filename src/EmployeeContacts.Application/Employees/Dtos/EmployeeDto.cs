namespace EmployeeContacts.Application.Employees.Dtos;

public sealed record EmployeeDto(
    Guid Id,
    string Name,
    string Email,
    string Tel,
    DateOnly Joined);
