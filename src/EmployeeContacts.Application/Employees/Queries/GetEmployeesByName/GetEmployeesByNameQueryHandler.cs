using EmployeeContacts.Application.Abstractions.Persistence;
using EmployeeContacts.Application.Employees.Dtos;
using MediatR;

namespace EmployeeContacts.Application.Employees.Queries.GetEmployeesByName;

public sealed class GetEmployeesByNameQueryHandler : IRequestHandler<GetEmployeesByNameQuery, IReadOnlyList<EmployeeDto>>
{
    private readonly IEmployeeRepository employeeRepository;

    public GetEmployeesByNameQueryHandler(IEmployeeRepository employeeRepository)
    {
        this.employeeRepository = employeeRepository;
    }

    public Task<IReadOnlyList<EmployeeDto>> Handle(GetEmployeesByNameQuery request, CancellationToken cancellationToken)
        => employeeRepository.GetByNameAsync(request.Name.Trim(), cancellationToken);
}
