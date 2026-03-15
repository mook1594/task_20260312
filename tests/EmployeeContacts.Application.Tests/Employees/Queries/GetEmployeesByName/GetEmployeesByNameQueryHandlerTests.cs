using EmployeeContacts.Application.Employees.Dtos;
using EmployeeContacts.Application.Employees.Queries.GetEmployeesByName;
using EmployeeContacts.Application.Tests.TestDoubles;

namespace EmployeeContacts.Application.Tests.Employees.Queries.GetEmployeesByName;

public class GetEmployeesByNameQueryHandlerTests
{
    [Fact(DisplayName = "이름 검색 전에 trim 처리한다.")]
    public async Task Handle_ShouldTrimNameBeforeSearching()
    {
        var repository = new TestEmployeeRepository
        {
            EmployeesByNameResult =
            [
                new EmployeeDto(Guid.NewGuid(), "김철수", "kim@example.com", "01012345678", new DateOnly(2020, 1, 1))
            ]
        };
        var handler = new GetEmployeesByNameQueryHandler(repository);

        IReadOnlyList<EmployeeDto> result = await handler.Handle(
            new GetEmployeesByNameQuery("  김철수  "),
            CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("김철수", repository.LastRequestedName);
        Assert.Equal(1, repository.GetByNameCallCount);
    }

    [Fact(DisplayName = "검색 결과가 없으면 빈 컬렉션을 반환한다.")]
    public async Task Handle_ShouldReturnEmptyList_WhenNoEmployeeExists()
    {
        var repository = new TestEmployeeRepository();
        var handler = new GetEmployeesByNameQueryHandler(repository);

        IReadOnlyList<EmployeeDto> result = await handler.Handle(
            new GetEmployeesByNameQuery("김철수"),
            CancellationToken.None);

        Assert.Empty(result);
    }
}
