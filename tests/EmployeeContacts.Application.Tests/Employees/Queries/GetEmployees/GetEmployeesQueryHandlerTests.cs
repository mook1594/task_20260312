using EmployeeContacts.Application.Common.Models;
using EmployeeContacts.Application.Employees.Dtos;
using EmployeeContacts.Application.Employees.Queries.GetEmployees;
using EmployeeContacts.Application.Tests.TestDoubles;

namespace EmployeeContacts.Application.Tests.Employees.Queries.GetEmployees;

public class GetEmployeesQueryHandlerTests
{
    [Fact(DisplayName = "요청한 page와 pageSize로 직원 목록을 조회한다.")]
    public async Task Handle_ShouldReturnPagedEmployees_WithRequestedPaging()
    {
        var expected = new PagedResult<EmployeeDto>(
            [new EmployeeDto(Guid.NewGuid(), "김철수", "kim@example.com", "01012345678", new DateOnly(2024, 2, 1))],
            1,
            20,
            1,
            1);
        var repository = new TestEmployeeRepository
        {
            PagedResult = expected
        };
        var handler = new GetEmployeesQueryHandler(repository);

        PagedResult<EmployeeDto> result = await handler.Handle(new GetEmployeesQuery(), CancellationToken.None);

        Assert.Equal(expected, result);
        Assert.Equal(1, repository.LastRequestedPage);
        Assert.Equal(20, repository.LastRequestedPageSize);
        Assert.Equal(1, repository.GetPagedCallCount);
    }
}
