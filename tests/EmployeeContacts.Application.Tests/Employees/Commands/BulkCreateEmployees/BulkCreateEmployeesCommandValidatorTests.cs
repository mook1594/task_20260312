using EmployeeContacts.Application.Employees.Commands.BulkCreateEmployees;
using FluentValidation.Results;

namespace EmployeeContacts.Application.Tests.Employees.Commands.BulkCreateEmployees;

public class BulkCreateEmployeesCommandValidatorTests
{
    private readonly BulkCreateEmployeesCommandValidator validator = new();

    [Fact(DisplayName = "Records가 null이면 검증에 실패한다.")]
    public void Validate_ShouldFail_WhenRecordsAreNull()
    {
        ValidationResult result = validator.Validate(new BulkCreateEmployeesCommand(null!));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Records");
    }

    [Fact(DisplayName = "Records가 비어 있으면 검증에 실패한다.")]
    public void Validate_ShouldFail_WhenRecordsAreEmpty()
    {
        ValidationResult result = validator.Validate(new BulkCreateEmployeesCommand([]));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Records");
    }

    [Fact(DisplayName = "행 번호가 1보다 작으면 검증에 실패한다.")]
    public void Validate_ShouldFail_WhenRowIsLessThanOne()
    {
        ValidationResult result = validator.Validate(new BulkCreateEmployeesCommand(
        [
            new BulkEmployeeRecord(0, "김철수", "kim@example.com", "01012345678", "2024-02-01")
        ]));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(".Row", StringComparison.Ordinal));
    }
}
