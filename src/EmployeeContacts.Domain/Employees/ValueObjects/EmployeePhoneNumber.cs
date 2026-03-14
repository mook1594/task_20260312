using EmployeeContacts.Domain.Common;
using EmployeeContacts.Domain.Employees.Errors;

namespace EmployeeContacts.Domain.Employees.ValueObjects;

public sealed class EmployeePhoneNumber : IEquatable<EmployeePhoneNumber>
{
    private EmployeePhoneNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static EmployeePhoneNumber Create(string value)
    {
        if (value is null)
        {
            throw new DomainException(EmployeeDomainErrors.PhoneNumberInvalid);
        }

        string trimmedValue = value.Trim();
        if (trimmedValue.Length == 0)
        {
            throw new DomainException(EmployeeDomainErrors.PhoneNumberInvalid);
        }

        foreach (char character in trimmedValue)
        {
            if (!char.IsDigit(character) && character != '-')
            {
                throw new DomainException(EmployeeDomainErrors.PhoneNumberInvalid);
            }
        }

        string normalizedValue = trimmedValue.Replace("-", string.Empty, StringComparison.Ordinal);
        if (normalizedValue.Length != 11 || !normalizedValue.StartsWith("010", StringComparison.Ordinal))
        {
            throw new DomainException(EmployeeDomainErrors.PhoneNumberInvalid);
        }

        return new EmployeePhoneNumber(normalizedValue);
    }

    public override string ToString() => Value;

    public bool Equals(EmployeePhoneNumber? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return StringComparer.Ordinal.Equals(Value, other.Value);
    }

    public override bool Equals(object? obj) => obj is EmployeePhoneNumber other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);
}
