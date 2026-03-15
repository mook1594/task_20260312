using System.Text.Json;
using EmployeeContacts.Application.Abstractions.Parsing;
using EmployeeContacts.Application.Employees.Commands.BulkCreateEmployees;
using EmployeeContacts.Infrastructure.Parsing;

namespace EmployeeContacts.Infrastructure.Parsing.Json;

public sealed class JsonEmployeeImportParser : IEmployeeImportParser
{
    public Task<IReadOnlyList<BulkEmployeeRecord>> ParseAsync(string content, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using JsonDocument document = JsonDocument.Parse(content);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                throw ParsingApplicationExceptionFactory.InvalidFormatException();
            }

            List<BulkEmployeeRecord> records = [];
            int rowNumber = 1;

            foreach (JsonElement element in document.RootElement.EnumerateArray())
            {
                if (element.ValueKind != JsonValueKind.Object)
                {
                    throw ParsingApplicationExceptionFactory.InvalidFormatException();
                }

                records.Add(new BulkEmployeeRecord(
                    rowNumber,
                    GetRequiredStringProperty(element, "name"),
                    GetRequiredStringProperty(element, "email"),
                    GetRequiredStringProperty(element, "tel"),
                    GetRequiredStringProperty(element, "joined")));
                rowNumber++;
            }

            if (records.Count == 0)
            {
                throw ParsingApplicationExceptionFactory.InvalidFormatException();
            }

            return Task.FromResult<IReadOnlyList<BulkEmployeeRecord>>(records);
        }
        catch (EmployeeContacts.Application.Common.Errors.ApplicationException)
        {
            throw;
        }
        catch (JsonException)
        {
            throw ParsingApplicationExceptionFactory.InvalidFormatException();
        }
    }

    private static string GetRequiredStringProperty(JsonElement element, string propertyName)
    {
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (!string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (property.Value.ValueKind != JsonValueKind.String)
            {
                throw ParsingApplicationExceptionFactory.InvalidFormatException();
            }

            return property.Value.GetString()
                ?? throw ParsingApplicationExceptionFactory.InvalidFormatException();
        }

        throw ParsingApplicationExceptionFactory.InvalidFormatException();
    }
}
