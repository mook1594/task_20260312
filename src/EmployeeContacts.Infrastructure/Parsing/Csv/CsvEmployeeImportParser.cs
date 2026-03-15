using EmployeeContacts.Application.Abstractions.Parsing;
using EmployeeContacts.Application.Employees.Commands.BulkCreateEmployees;

namespace EmployeeContacts.Infrastructure.Parsing.Csv;

public sealed class CsvEmployeeImportParser : IEmployeeImportParser
{
    private static readonly string[] HeaderColumns = ["name", "email", "tel", "joined"];

    public Task<IReadOnlyList<BulkEmployeeRecord>> ParseAsync(string content, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw ParsingApplicationExceptionFactory.InvalidFormatException();
        }

        string normalizedContent = content.TrimStart('\uFEFF');
        string[] lines = normalizedContent.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');

        if (lines.Length > 0 && lines[^1].Length == 0)
        {
            Array.Resize(ref lines, lines.Length - 1);
        }

        if (lines.Length == 0)
        {
            throw ParsingApplicationExceptionFactory.InvalidFormatException();
        }

        bool hasHeader = IsHeader(lines[0]);
        int rowNumber = 1;
        int startIndex = hasHeader ? 1 : 0;
        List<BulkEmployeeRecord> records = [];

        for (int index = startIndex; index < lines.Length; index++)
        {
            string line = lines[index];
            if (line.Length == 0)
            {
                throw ParsingApplicationExceptionFactory.InvalidFormatException();
            }

            if (line.Contains('"'))
            {
                throw ParsingApplicationExceptionFactory.InvalidFormatException();
            }

            string[] columns = line.Split(',');
            if (columns.Length != 4)
            {
                throw ParsingApplicationExceptionFactory.InvalidFormatException();
            }

            records.Add(new BulkEmployeeRecord(
                rowNumber,
                columns[0],
                columns[1],
                columns[2],
                columns[3]));
            rowNumber++;
        }

        if (records.Count == 0)
        {
            throw ParsingApplicationExceptionFactory.InvalidFormatException();
        }

        return Task.FromResult<IReadOnlyList<BulkEmployeeRecord>>(records);
    }

    private static bool IsHeader(string line)
    {
        string[] columns = line.Split(',');
        if (columns.Length != 4)
        {
            return false;
        }

        bool matchesExpectedHeader = HeaderColumns
            .Select((column, index) => string.Equals(columns[index], column, StringComparison.OrdinalIgnoreCase))
            .All(matches => matches);

        if (matchesExpectedHeader)
        {
            return true;
        }

        bool looksLikeHeader = HeaderColumns.Any(expected =>
            columns.Any(column => string.Equals(column, expected, StringComparison.OrdinalIgnoreCase)));

        if (looksLikeHeader)
        {
            throw ParsingApplicationExceptionFactory.InvalidFormatException();
        }

        return false;
    }
}
