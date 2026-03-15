using EmployeeContacts.Application.Common.Models;
using EmployeeContacts.Application.Employees.Dtos;
using MediatR;

namespace EmployeeContacts.Application.Employees.Queries.GetEmployees;

public sealed record GetEmployeesQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<EmployeeDto>>;
