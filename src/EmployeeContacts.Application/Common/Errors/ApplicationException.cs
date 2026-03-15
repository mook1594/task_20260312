namespace EmployeeContacts.Application.Common.Errors;

public sealed class ApplicationException : Exception
{
    public ApplicationException(ApplicationError error)
        : base(error.Detail)
    {
        Code = error.Code;
        Detail = error.Detail;
    }

    public string Code { get; }

    public string Detail { get; }
}
