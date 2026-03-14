using EmployeeContacts.Domain.Common;
using EmployeeContacts.Domain.Employees.Errors;
using EmployeeContacts.Domain.Employees.ValueObjects;

namespace EmployeeContacts.Domain.Tests.Employees.ValueObjects;

public class EmployeeNameTests
{
    [Fact(DisplayName = "이름 생성 시 앞뒤 공백을 제거한다.")]
    public void Create_ShouldTrimName()
    {
        var name = EmployeeName.Create("  김철수  ");

        Assert.Equal("김철수", name.Value);
    }

    [Fact(DisplayName = "이름이 null이면 예외를 던진다.")]
    public void Create_ShouldThrow_WhenNameIsNull()
    {
        DomainException exception = Assert.Throws<DomainException>(() => EmployeeName.Create(null!));

        Assert.Equal(EmployeeDomainErrors.NameRequired.Code, exception.Code);
        Assert.Equal(EmployeeDomainErrors.NameRequired.Detail, exception.Detail);
    }

    [Fact(DisplayName = "이름이 공백만 있으면 예외를 던진다.")]
    public void Create_ShouldThrow_WhenNameIsWhitespace()
    {
        DomainException exception = Assert.Throws<DomainException>(() => EmployeeName.Create("   "));

        Assert.Equal(EmployeeDomainErrors.NameRequired.Code, exception.Code);
        Assert.Equal(EmployeeDomainErrors.NameRequired.Detail, exception.Detail);
    }

    [Fact(DisplayName = "같은 정규화 결과를 가진 이름 값 객체는 동등하다.")]
    public void Create_ShouldBeEqual_WhenNormalizedValueMatches()
    {
        var first = EmployeeName.Create(" 김철수");
        var second = EmployeeName.Create("김철수 ");

        Assert.Equal(first, second);
        Assert.Equal("김철수", first.ToString());
    }
}
