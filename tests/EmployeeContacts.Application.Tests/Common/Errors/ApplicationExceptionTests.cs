using EmployeeContacts.Application.Common.Errors;

namespace EmployeeContacts.Application.Tests.Common.Errors;

public class ApplicationExceptionTests
{
    [Fact(DisplayName = "ApplicationException은 code, detail, message를 에러와 동일하게 노출한다.")]
    public void Constructor_ShouldExposeErrorMetadata()
    {
        var error = new ApplicationError("duplicate_email", "email already exists");

        var exception = new EmployeeContacts.Application.Common.Errors.ApplicationException(error);

        Assert.Equal("duplicate_email", exception.Code);
        Assert.Equal("email already exists", exception.Detail);
        Assert.Equal("email already exists", exception.Message);
    }
}
