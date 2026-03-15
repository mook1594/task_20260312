using EmployeeContacts.Application.Abstractions.Persistence;
using EmployeeContacts.Application.Common.Models;
using EmployeeContacts.Application.Employees.Dtos;
using MediatR;

namespace EmployeeContacts.Application.Employees.Queries.GetEmployees;

public sealed class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, PagedResult<EmployeeDto>>
{
    private readonly IEmployeeRepository employeeRepository;

    public GetEmployeesQueryHandler(IEmployeeRepository employeeRepository)
    {
        this.employeeRepository = employeeRepository;
    }

    public Task<PagedResult<EmployeeDto>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
        => employeeRepository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
}
