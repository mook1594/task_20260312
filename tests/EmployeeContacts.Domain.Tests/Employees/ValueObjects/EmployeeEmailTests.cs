using EmployeeContacts.Domain.Common;
using EmployeeContacts.Domain.Employees.Errors;
using EmployeeContacts.Domain.Employees.ValueObjects;

namespace EmployeeContacts.Domain.Tests.Employees.ValueObjects;

public class EmployeeEmailTests
{
    [Fact(DisplayName = "이메일 생성 시 앞뒤 공백을 제거하고 소문자로 정규화한다.")]
    public void Create_ShouldTrimAndLowercaseEmail()
    {
        var email = EmployeeEmail.Create(" Alice@Example.Com ");

        Assert.Equal("alice@example.com", email.Value);
    }

    [Fact(DisplayName = "이메일이 null이면 예외를 던진다.")]
    public void Create_ShouldThrow_WhenEmailIsNull()
    {
        AssertInvalid(null!);
    }

    [Fact(DisplayName = "이메일이 공백만 있으면 예외를 던진다.")]
    public void Create_ShouldThrow_WhenEmailIsWhitespace()
    {
        AssertInvalid("   ");
    }

    [Fact(DisplayName = "이메일에 @ 기호가 없으면 예외를 던진다.")]
    public void Create_ShouldThrow_WhenEmailDoesNotContainAtSymbol()
    {
        AssertInvalid("alice.example.com");
    }

    [Fact(DisplayName = "이메일에 @ 기호가 여러 개면 예외를 던진다.")]
    public void Create_ShouldThrow_WhenEmailContainsMultipleAtSymbols()
    {
        AssertInvalid("alice@@example.com");
    }

    [Fact(DisplayName = "이메일 도메인 파트가 비어 있으면 예외를 던진다.")]
    public void Create_ShouldThrow_WhenEmailHasNoDomainPart()
    {
        AssertInvalid("alice@");
    }

    [Fact(DisplayName = "이메일 도메인에 점이 없으면 예외를 던진다.")]
    public void Create_ShouldThrow_WhenEmailDomainHasNoDot()
    {
        AssertInvalid("alice@example");
    }

    [Fact(DisplayName = "같은 정규화 결과를 가진 이메일 값 객체는 동등하다.")]
    public void Create_ShouldBeEqual_WhenNormalizedValueMatches()
    {
        var first = EmployeeEmail.Create(" Alice@Example.Com ");
        var second = EmployeeEmail.Create("alice@example.com");

        Assert.Equal(first, second);
        Assert.Equal("alice@example.com", first.ToString());
    }

    private static void AssertInvalid(string email)
    {
        DomainException exception = Assert.Throws<DomainException>(() => EmployeeEmail.Create(email));

        Assert.Equal(EmployeeDomainErrors.EmailInvalid.Code, exception.Code);
        Assert.Equal(EmployeeDomainErrors.EmailInvalid.Detail, exception.Detail);
    }
}
