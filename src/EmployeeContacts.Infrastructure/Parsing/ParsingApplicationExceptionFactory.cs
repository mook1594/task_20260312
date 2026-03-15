using EmployeeContacts.Application.Common.Errors;

namespace EmployeeContacts.Infrastructure.Parsing;

internal static class ParsingApplicationExceptionFactory
{
    private static readonly ApplicationError InvalidFormat =
        new("invalid_format", "Request body format is invalid.");

    public static EmployeeContacts.Application.Common.Errors.ApplicationException InvalidFormatException(string? detail = null)
        => new(detail is null ? InvalidFormat : new ApplicationError(InvalidFormat.Code, detail));
}
