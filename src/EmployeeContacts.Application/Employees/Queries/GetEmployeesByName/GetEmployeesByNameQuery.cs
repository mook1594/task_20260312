using EmployeeContacts.Application.Employees.Dtos;
using MediatR;

namespace EmployeeContacts.Application.Employees.Queries.GetEmployeesByName;

public sealed record GetEmployeesByNameQuery(string Name) : IRequest<IReadOnlyList<EmployeeDto>>;
