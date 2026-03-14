using EmployeeContacts.Domain.Common;
using EmployeeContacts.Domain.Employees;
using EmployeeContacts.Domain.Employees.Errors;
using EmployeeContacts.Domain.Employees.ValueObjects;

namespace EmployeeContacts.Domain.Tests.Employees;

public class EmployeeTests
{
    [Fact(DisplayName = "직원 생성 시 정규화된 값으로 엔터티를 만든다.")]
    public void Create_ShouldBuildEmployee_WithNormalizedValues()
    {
        var id = Guid.NewGuid();
        var joined = new DateOnly(2024, 2, 1);
        var name = EmployeeName.Create("  김철수  ");
        var email = EmployeeEmail.Create(" Alice@Example.Com ");
        var phoneNumber = EmployeePhoneNumber.Create("010-1234-5678");

        var employee = Employee.Create(id, name, email, phoneNumber, joined);

        Assert.Equal(id, employee.Id);
        Assert.Equal(name, employee.Name);
        Assert.Equal(email, employee.Email);
        Assert.Equal(phoneNumber, employee.PhoneNumber);
        Assert.Equal("김철수", employee.Name.Value);
        Assert.Equal("alice@example.com", employee.Email.Value);
        Assert.Equal("01012345678", employee.PhoneNumber.Value);
        Assert.Equal(joined, employee.Joined);
        Assert.Equal(employee.CreatedAt, employee.UpdatedAt);
        Assert.Equal(TimeSpan.Zero, employee.CreatedAt.Offset);
    }

    [Fact(DisplayName = "입사일이 기본값이면 예외를 던진다.")]
    public void Create_ShouldThrow_WhenJoinedIsDefault()
    {
        var name = EmployeeName.Create("김철수");
        var email = EmployeeEmail.Create("alice@example.com");
        var phoneNumber = EmployeePhoneNumber.Create("01012345678");

        DomainException exception = Assert.Throws<DomainException>(
            () => Employee.Create(Guid.NewGuid(), name, email, phoneNumber, DateOnly.MinValue));

        Assert.Equal(EmployeeDomainErrors.JoinedRequired.Code, exception.Code);
        Assert.Equal(EmployeeDomainErrors.JoinedRequired.Detail, exception.Detail);
    }

    [Fact(DisplayName = "직원 생성 시 null 값 객체는 도메인 예외를 던진다.")]
    public void Create_ShouldThrowDomainException_WhenNameIsNull()
    {
        DomainException exception = Assert.Throws<DomainException>(
            () => Employee.Create(
                Guid.NewGuid(),
                null!,
                EmployeeEmail.Create("alice@example.com"),
                EmployeePhoneNumber.Create("01012345678"),
                new DateOnly(2024, 2, 1)));

        Assert.Equal(EmployeeDomainErrors.NameRequired.Code, exception.Code);
        Assert.Equal(EmployeeDomainErrors.NameRequired.Detail, exception.Detail);
    }

    [Fact(DisplayName = "직원 생성 시 null 이메일 값 객체는 도메인 예외를 던진다.")]
    public void Create_ShouldThrowDomainException_WhenEmailIsNull()
    {
        DomainException exception = Assert.Throws<DomainException>(
            () => Employee.Create(
                Guid.NewGuid(),
                EmployeeName.Create("김철수"),
                null!,
                EmployeePhoneNumber.Create("01012345678"),
                new DateOnly(2024, 2, 1)));

        Assert.Equal(EmployeeDomainErrors.EmailInvalid.Code, exception.Code);
        Assert.Equal(EmployeeDomainErrors.EmailInvalid.Detail, exception.Detail);
    }

    [Fact(DisplayName = "직원 생성 시 null 전화번호 값 객체는 도메인 예외를 던진다.")]
    public void Create_ShouldThrowDomainException_WhenPhoneNumberIsNull()
    {
        DomainException exception = Assert.Throws<DomainException>(
            () => Employee.Create(
                Guid.NewGuid(),
                EmployeeName.Create("김철수"),
                EmployeeEmail.Create("alice@example.com"),
                null!,
                new DateOnly(2024, 2, 1)));

        Assert.Equal(EmployeeDomainErrors.PhoneNumberInvalid.Code, exception.Code);
        Assert.Equal(EmployeeDomainErrors.PhoneNumberInvalid.Detail, exception.Detail);
    }

    [Fact(DisplayName = "이름 중복은 도메인 금지 규칙이 아니므로 허용한다.")]
    public void Create_ShouldAllowDuplicateNamesAsDomainConcern()
    {
        var joined = new DateOnly(2024, 2, 1);
        var firstName = EmployeeName.Create("김철수");
        var secondName = EmployeeName.Create("김철수");

        var first = Employee.Create(
            Guid.NewGuid(),
            firstName,
            EmployeeEmail.Create("first@example.com"),
            EmployeePhoneNumber.Create("01011112222"),
            joined);
        var second = Employee.Create(
            Guid.NewGuid(),
            secondName,
            EmployeeEmail.Create("second@example.com"),
            EmployeePhoneNumber.Create("01033334444"),
            joined);

        Assert.Equal(firstName, first.Name);
        Assert.Equal(secondName, second.Name);
        Assert.Equal("김철수", first.Name.Value);
        Assert.Equal("김철수", second.Name.Value);
    }
}
