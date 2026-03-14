using EmployeeContacts.Domain.Common;
using EmployeeContacts.Domain.Employees.Errors;
using EmployeeContacts.Domain.Employees.ValueObjects;

namespace EmployeeContacts.Domain.Tests.Employees.ValueObjects;

public class EmployeePhoneNumberTests
{
    [Fact(DisplayName = "하이픈이 포함된 전화번호를 숫자만 남도록 정규화한다.")]
    public void Create_ShouldNormalizeHyphenatedPhoneNumber()
    {
        var phoneNumber = EmployeePhoneNumber.Create("010-1234-5678");

        Assert.Equal("01012345678", phoneNumber.Value);
    }

    [Fact(DisplayName = "유효한 전화번호 입력은 숫자만 저장한다.")]
    public void Create_ShouldKeepDigitsOnlyForValidInput()
    {
        var phoneNumber = EmployeePhoneNumber.Create("01012345678");

        Assert.Equal("01012345678", phoneNumber.Value);
    }

    [Fact(DisplayName = "전화번호에 숫자와 하이픈 외 문자가 있으면 예외를 던진다.")]
    public void Create_ShouldThrow_WhenPhoneContainsInvalidCharacters()
    {
        AssertInvalid("010-12A4-5678");
    }

    [Fact(DisplayName = "전화번호가 010으로 시작하지 않으면 예외를 던진다.")]
    public void Create_ShouldThrow_WhenPhoneDoesNotStartWith010()
    {
        AssertInvalid("01112345678");
    }

    [Fact(DisplayName = "전화번호 길이가 11자리가 아니면 예외를 던진다.")]
    public void Create_ShouldThrow_WhenPhoneLengthIsNot11()
    {
        AssertInvalid("0101234567");
        AssertInvalid("010123456789");
        AssertInvalid("010-123-5678");
    }

    [Fact(DisplayName = "같은 정규화 결과를 가진 전화번호 값 객체는 동등하다.")]
    public void Create_ShouldBeEqual_WhenNormalizedValueMatches()
    {
        var first = EmployeePhoneNumber.Create("010-1234-5678");
        var second = EmployeePhoneNumber.Create("01012345678");

        Assert.Equal(first, second);
        Assert.Equal("01012345678", first.ToString());
    }

    private static void AssertInvalid(string phoneNumber)
    {
        DomainException exception = Assert.Throws<DomainException>(() => EmployeePhoneNumber.Create(phoneNumber));

        Assert.Equal(EmployeeDomainErrors.PhoneNumberInvalid.Code, exception.Code);
        Assert.Equal(EmployeeDomainErrors.PhoneNumberInvalid.Detail, exception.Detail);
    }
}
