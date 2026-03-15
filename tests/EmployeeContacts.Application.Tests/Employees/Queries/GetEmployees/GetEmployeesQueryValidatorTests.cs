using EmployeeContacts.Application.Employees.Queries.GetEmployees;
using FluentValidation.Results;

namespace EmployeeContacts.Application.Tests.Employees.Queries.GetEmployees;

public class GetEmployeesQueryValidatorTests
{
    private readonly GetEmployeesQueryValidator validator = new();

    [Fact(DisplayName = "page가 1보다 작으면 검증에 실패한다.")]
    public void Validate_ShouldFail_WhenPageIsLessThanOne()
    {
        ValidationResult result = validator.Validate(new GetEmployeesQuery(0, 20));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Page");
    }

    [Fact(DisplayName = "pageSize가 100보다 크면 검증에 실패한다.")]
    public void Validate_ShouldFail_WhenPageSizeExceedsMaximum()
    {
        ValidationResult result = validator.Validate(new GetEmployeesQuery(1, 101));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "PageSize");
    }

    [Fact(DisplayName = "기본 page와 pageSize는 유효하다.")]
    public void Validate_ShouldSucceed_WithDefaultPaging()
    {
        ValidationResult result = validator.Validate(new GetEmployeesQuery());

        Assert.True(result.IsValid);
    }
}
