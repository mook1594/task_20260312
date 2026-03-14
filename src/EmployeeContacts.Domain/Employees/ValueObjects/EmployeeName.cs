using EmployeeContacts.Domain.Common;
using EmployeeContacts.Domain.Employees.Errors;

namespace EmployeeContacts.Domain.Employees.ValueObjects;

public sealed class EmployeeName : IEquatable<EmployeeName>
{
    private EmployeeName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static EmployeeName Create(string value)
    {
        if (value is null)
        {
            throw new DomainException(EmployeeDomainErrors.NameRequired);
        }

        string normalizedValue = value.Trim();

        if (normalizedValue.Length == 0)
        {
            throw new DomainException(EmployeeDomainErrors.NameRequired);
        }

        return new EmployeeName(normalizedValue);
    }

    public override string ToString() => Value;

    public bool Equals(EmployeeName? other)
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

    public override bool Equals(object? obj) => obj is EmployeeName other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);
}
