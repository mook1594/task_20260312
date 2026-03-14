using EmployeeContacts.Domain.Common;

namespace EmployeeContacts.Domain.Tests.Common;

public class DomainExceptionTests
{
    [Fact(DisplayName = "도메인 예외는 코드와 상세 메시지를 유지한다.")]
    public void Constructor_ShouldPreserveCodeAndDetail()
    {
        var error = new DomainError("invalid_name", "Employee name is required.");

        var exception = new DomainException(error);

        Assert.Equal("invalid_name", exception.Code);
        Assert.Equal("Employee name is required.", exception.Detail);
        Assert.Equal("Employee name is required.", exception.Message);
    }
}
