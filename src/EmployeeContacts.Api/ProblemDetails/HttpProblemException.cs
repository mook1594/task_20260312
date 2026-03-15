namespace EmployeeContacts.Api.ProblemDetails;

public sealed class HttpProblemException : Exception
{
    public HttpProblemException(int statusCode, string title, string detail)
        : base(detail)
    {
        StatusCode = statusCode;
        Title = title;
        Detail = detail;
    }

    public int StatusCode { get; }

    public string Title { get; }

    public string Detail { get; }
}
