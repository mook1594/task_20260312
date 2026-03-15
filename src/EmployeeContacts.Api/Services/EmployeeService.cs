using EmployeeContacts.Api.ProblemDetails;
using EmployeeContacts.Application.Abstractions.Parsing;
using EmployeeContacts.Application.Employees.Commands.BulkCreateEmployees;
using EmployeeContacts.Infrastructure.Parsing.Csv;
using EmployeeContacts.Infrastructure.Parsing.Json;
using Microsoft.Net.Http.Headers;
using System.Net.Mime;
using System.Text;

namespace EmployeeContacts.Api.Services;

public sealed class EmployeeService
{
    private readonly IPlainTextEmployeeImportDetector plainTextDetector;
    private readonly CsvEmployeeImportParser csvParser;
    private readonly JsonEmployeeImportParser jsonParser;

    public EmployeeService(
        IPlainTextEmployeeImportDetector plainTextDetector,
        CsvEmployeeImportParser csvParser,
        JsonEmployeeImportParser jsonParser)
    {
        this.plainTextDetector = plainTextDetector ?? throw new ArgumentNullException(nameof(plainTextDetector));
        this.csvParser = csvParser ?? throw new ArgumentNullException(nameof(csvParser));
        this.jsonParser = jsonParser ?? throw new ArgumentNullException(nameof(jsonParser));
    }

    public async Task<IReadOnlyList<BulkEmployeeRecord>> ParseRecordsAsync(
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        string mediaType = GetMediaType(request.ContentType);
        return mediaType switch
        {
            MediaTypeNames.Application.Json => await ParseAsync(
                    request,
                    await ReadBodyAsync(request.Body, cancellationToken).ConfigureAwait(false),
                    cancellationToken)
                .ConfigureAwait(false),
            "text/csv" => await ParseAsync(
                    request,
                    await ReadBodyAsync(request.Body, cancellationToken).ConfigureAwait(false),
                    cancellationToken)
                .ConfigureAwait(false),
            MediaTypeNames.Text.Plain => await ParsePlainTextAsync(request, cancellationToken).ConfigureAwait(false),
            "multipart/form-data" => await ParseMultipartAsync(request, cancellationToken).ConfigureAwait(false),
            _ => throw new HttpProblemException(
                StatusCodes.Status415UnsupportedMediaType,
                "Unsupported Media Type",
                $"Content-Type '{mediaType}' is not supported.")
        };
    }

    private async Task<IReadOnlyList<BulkEmployeeRecord>> ParsePlainTextAsync(
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        string content = await ReadBodyAsync(request.Body, cancellationToken).ConfigureAwait(false);
        IEmployeeImportParser parser = plainTextDetector.Resolve(content);

        return await ParseAsync(parser, content, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<BulkEmployeeRecord>> ParseMultipartAsync(
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        IFormCollection form = await request.ReadFormAsync(cancellationToken).ConfigureAwait(false);
        if (form.Files.Count != 1 || form.Count != 0)
        {
            throw new HttpProblemException(
                StatusCodes.Status400BadRequest,
                "Bad Request",
                "multipart/form-data requests must include exactly one employeesFile part.");
        }

        IFormFile file = form.Files[0];
        if (!string.Equals(file.Name, "employeesFile", StringComparison.Ordinal))
        {
            throw new HttpProblemException(
                StatusCodes.Status400BadRequest,
                "Bad Request",
                "multipart/form-data requests must include the employeesFile part.");
        }

        await using Stream stream = file.OpenReadStream();
        string content = await ReadBodyAsync(stream, cancellationToken).ConfigureAwait(false);
        IEmployeeImportParser parser = plainTextDetector.Resolve(content);

        return await ParseAsync(parser, content, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<BulkEmployeeRecord>> ParseAsync(
        HttpRequest request,
        string content,
        CancellationToken cancellationToken)
    {
        string mediaType = GetMediaType(request.ContentType);
        IEmployeeImportParser parser = mediaType switch
        {
            MediaTypeNames.Application.Json => jsonParser,
            "text/csv" => csvParser,
            _ => throw new HttpProblemException(
                StatusCodes.Status415UnsupportedMediaType,
                "Unsupported Media Type",
                $"Content-Type '{mediaType}' is not supported.")
        };

        return await ParseAsync(parser, content, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<IReadOnlyList<BulkEmployeeRecord>> ParseAsync(
        IEmployeeImportParser parser,
        string content,
        CancellationToken cancellationToken)
        => await parser.ParseAsync(content, cancellationToken).ConfigureAwait(false);

    private static async Task<string> ReadBodyAsync(Stream stream, CancellationToken cancellationToken)
    {
        using StreamReader reader = new(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        return await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string GetMediaType(string? contentType)
    {
        if (MediaTypeHeaderValue.TryParse(contentType, out MediaTypeHeaderValue? mediaTypeHeaderValue)
            && !string.IsNullOrWhiteSpace(mediaTypeHeaderValue.MediaType.Value))
        {
            return mediaTypeHeaderValue.MediaType.Value;
        }

        return string.Empty;
    }
}
