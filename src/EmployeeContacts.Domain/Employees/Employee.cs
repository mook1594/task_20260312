using EmployeeContacts.Domain.Common;
using EmployeeContacts.Domain.Employees.Errors;
using EmployeeContacts.Domain.Employees.ValueObjects;

namespace EmployeeContacts.Domain.Employees;

public sealed class Employee
{
    private Employee(
        Guid id,
        EmployeeName name,
        EmployeeEmail email,
        EmployeePhoneNumber phoneNumber,
        DateOnly joined,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        Id = id;
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
        Joined = joined;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; }

    public EmployeeName Name { get; }

    public EmployeeEmail Email { get; }

    public EmployeePhoneNumber PhoneNumber { get; }

    public DateOnly Joined { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Employee Create(
        Guid id,
        EmployeeName name,
        EmployeeEmail email,
        EmployeePhoneNumber phoneNumber,
        DateOnly joined)
    {
        if (joined == DateOnly.MinValue)
        {
            throw new DomainException(EmployeeDomainErrors.JoinedRequired);
        }

        if (name is null)
        {
            throw new DomainException(EmployeeDomainErrors.NameRequired);
        }

        if (email is null)
        {
            throw new DomainException(EmployeeDomainErrors.EmailInvalid);
        }

        if (phoneNumber is null)
        {
            throw new DomainException(EmployeeDomainErrors.PhoneNumberInvalid);
        }

        DateTimeOffset timestamp = DateTimeOffset.UtcNow;

        return new Employee(id, name, email, phoneNumber, joined, timestamp, timestamp);
    }
}
