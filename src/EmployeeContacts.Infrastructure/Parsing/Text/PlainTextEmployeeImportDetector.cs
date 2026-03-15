using System.Text.Json;
using EmployeeContacts.Application.Abstractions.Parsing;
using EmployeeContacts.Infrastructure.Parsing.Csv;
using EmployeeContacts.Infrastructure.Parsing.Json;

namespace EmployeeContacts.Infrastructure.Parsing.Text;

public sealed class PlainTextEmployeeImportDetector : IPlainTextEmployeeImportDetector
{
    private readonly CsvEmployeeImportParser csvParser;
    private readonly JsonEmployeeImportParser jsonParser;

    public PlainTextEmployeeImportDetector(
        CsvEmployeeImportParser csvParser,
        JsonEmployeeImportParser jsonParser)
    {
        this.csvParser = csvParser ?? throw new ArgumentNullException(nameof(csvParser));
        this.jsonParser = jsonParser ?? throw new ArgumentNullException(nameof(jsonParser));
    }

    public IEmployeeImportParser Resolve(string content)
    {
        if (LooksLikeJsonArray(content))
        {
            return jsonParser;
        }

        return csvParser;
    }

    private static bool LooksLikeJsonArray(string content)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(content);
            return document.RootElement.ValueKind == JsonValueKind.Array;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
