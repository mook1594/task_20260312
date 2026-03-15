using EmployeeContacts.Application.Common.Errors;
using EmployeeContacts.Domain.Common;
using EmployeeContacts.Domain.Employees.Errors;

namespace EmployeeContacts.Application.Employees.Commands.BulkCreateEmployees;

internal static class BulkCreateEmployeeErrorMapper
{
    public static BulkCreateEmployeesError ToContract(BulkCreateEmployeeRowError error)
        => new(error.Row, error.Field, error.Error.Code, error.Error.Detail);

    public static BulkCreateEmployeeRowError FromDomainException(int row, DomainException exception)
    {
        if (exception.Code == EmployeeDomainErrors.NameRequired.Code)
        {
            return Create(row, "name", new ApplicationError(exception.Code, exception.Detail));
        }

        if (exception.Code == EmployeeDomainErrors.EmailInvalid.Code)
        {
            return Create(row, "email", new ApplicationError(exception.Code, exception.Detail));
        }

        if (exception.Code == EmployeeDomainErrors.PhoneNumberInvalid.Code)
        {
            return Create(row, "tel", new ApplicationError(exception.Code, exception.Detail));
        }

        if (exception.Code == EmployeeDomainErrors.JoinedRequired.Code)
        {
            return Create(row, "joined", BulkCreateEmployeeErrors.InvalidJoined);
        }

        throw new EmployeeContacts.Application.Common.Errors.ApplicationException(
            new ApplicationError("unmapped_domain_error", $"Bulk create does not map domain error code '{exception.Code}'."));
    }

    public static BulkCreateEmployeeRowError InvalidJoined(int row)
        => Create(row, "joined", BulkCreateEmployeeErrors.InvalidJoined);

    public static BulkCreateEmployeeRowError DuplicateEmail(int row)
        => Create(row, "email", BulkCreateEmployeeErrors.DuplicateEmail);

    public static BulkCreateEmployeeRowError DuplicateTel(int row)
        => Create(row, "tel", BulkCreateEmployeeErrors.DuplicateTel);

    private static BulkCreateEmployeeRowError Create(int row, string field, ApplicationError error)
        => new(row, field, error);

    internal sealed record BulkCreateEmployeeRowError(int Row, string Field, ApplicationError Error);
}
