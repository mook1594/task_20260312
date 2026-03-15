namespace EmployeeContacts.Application.Abstractions.Parsing;

public interface IPlainTextEmployeeImportDetector
{
    IEmployeeImportParser Resolve(string content);
}
