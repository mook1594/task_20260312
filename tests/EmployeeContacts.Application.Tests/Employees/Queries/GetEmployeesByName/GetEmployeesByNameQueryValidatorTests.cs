using EmployeeContacts.Application.Employees.Queries.GetEmployeesByName;

namespace EmployeeContacts.Application.Tests.Employees.Queries.GetEmployeesByName;

public class GetEmployeesByNameQueryValidatorTests
{
    private readonly GetEmployeesByNameQueryValidator validator = new();

    [Fact(DisplayName = "trim 결과가 비어 있으면 검증에 실패한다.")]
    public void Validate_ShouldFail_WhenNameIsEmptyAfterTrim()
    {
        var result = validator.Validate(new GetEmployeesByNameQuery("   "));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Name");
    }
}
