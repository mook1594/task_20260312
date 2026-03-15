using MediatR;

namespace EmployeeContacts.Application.Employees.Commands.BulkCreateEmployees;

public sealed record BulkCreateEmployeesCommand(
    IReadOnlyList<BulkEmployeeRecord> Records) : IRequest<BulkCreateEmployeesResult>;
