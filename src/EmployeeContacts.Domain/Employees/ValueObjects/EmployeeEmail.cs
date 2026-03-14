using EmployeeContacts.Domain.Common;
using EmployeeContacts.Domain.Employees.Errors;

namespace EmployeeContacts.Domain.Employees.ValueObjects;

public sealed class EmployeeEmail : IEquatable<EmployeeEmail>
{
    private EmployeeEmail(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static EmployeeEmail Create(string value)
    {
        if (value is null)
        {
            throw new DomainException(EmployeeDomainErrors.EmailInvalid);
        }

        string normalizedValue = value.Trim().ToLowerInvariant();

        if (normalizedValue.Length == 0)
        {
            throw new DomainException(EmployeeDomainErrors.EmailInvalid);
        }

        string[] segments = normalizedValue.Split('@');
        if (segments.Length != 2)
        {
            throw new DomainException(EmployeeDomainErrors.EmailInvalid);
        }

        string localPart = segments[0];
        string domainPart = segments[1];

        if (localPart.Length == 0 || domainPart.Length == 0 || !domainPart.Contains('.'))
        {
            throw new DomainException(EmployeeDomainErrors.EmailInvalid);
        }

        return new EmployeeEmail(normalizedValue);
    }

    public override string ToString() => Value;

    public bool Equals(EmployeeEmail? other)
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

    public override bool Equals(object? obj) => obj is EmployeeEmail other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);
}
