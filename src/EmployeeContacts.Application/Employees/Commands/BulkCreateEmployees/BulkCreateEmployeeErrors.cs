using EmployeeContacts.Application.Common.Errors;

namespace EmployeeContacts.Application.Employees.Commands.BulkCreateEmployees;

internal static class BulkCreateEmployeeErrors
{
    public static readonly ApplicationError InvalidJoined = new("invalid_joined", "joined must be yyyy-MM-dd");
    public static readonly ApplicationError DuplicateEmail = new("duplicate_email", "email already exists");
    public static readonly ApplicationError DuplicateTel = new("duplicate_tel", "tel already exists");
}
