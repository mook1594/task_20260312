using EmployeeContacts.Application.Abstractions.Parsing;

namespace EmployeeContacts.Api.IntegrationTests.TestCommon;

internal sealed class ThrowingPlainTextEmployeeImportDetector : IPlainTextEmployeeImportDetector
{
    public IEmployeeImportParser Resolve(string content)
        => throw new InvalidOperationException("boom");
}
