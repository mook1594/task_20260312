using EmployeeContacts.Application.Common.Models;

namespace EmployeeContacts.Application.Tests.Common.Models;

public class PagedResultTests
{
    [Fact(DisplayName = "TotalCount가 0이면 TotalPages는 항상 0이다.")]
    public void Constructor_ShouldSetTotalPagesToZero_WhenTotalCountIsZero()
    {
        var result = new PagedResult<string>([], 1, 20, 0, 5);

        Assert.Equal(0, result.TotalPages);
    }
}
