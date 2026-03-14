using EmployeeContacts.Domain.Common;

namespace EmployeeContacts.Domain.Employees.Errors;

public static class EmployeeDomainErrors
{
    public static readonly DomainError NameRequired = new("invalid_name", "Employee name is required.");
    public static readonly DomainError EmailInvalid = new("invalid_email", "Employee email is invalid.");
    public static readonly DomainError PhoneNumberInvalid =
        new("invalid_tel", "Employee phone number must be an 11-digit mobile number starting with 010.");
    public static readonly DomainError JoinedRequired = new("invalid_joined", "Employee joined date is required.");
}
