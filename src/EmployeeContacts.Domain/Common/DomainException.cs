namespace EmployeeContacts.Domain.Common;

public sealed class DomainException : Exception
{
    public DomainException(DomainError error)
        : base(error.Detail)
    {
        Code = error.Code;
        Detail = error.Detail;
    }

    public string Code { get; }

    public string Detail { get; }
}
