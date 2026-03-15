using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EmployeeContacts.Api.OpenApi;

public sealed class EmployeeImportOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        if (!string.Equals(context.MethodInfo.Name, "BulkCreateEmployees", StringComparison.Ordinal))
        {
            return;
        }

        operation.RequestBody = new OpenApiRequestBody
        {
            Required = true,
            Content = new Dictionary<string, OpenApiMediaType>(StringComparer.OrdinalIgnoreCase)
            {
                ["application/json"] = new()
                {
                    Schema = CreateJsonSchema()
                },
                ["text/plain"] = new()
                {
                    Schema = CreatePlainTextSchema()
                },
                ["text/csv"] = new()
                {
                    Schema = CreatePlainTextSchema()
                },
                ["multipart/form-data"] = new()
                {
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                        Required = new HashSet<string> { "employeesFile" },
                        Properties = new Dictionary<string, IOpenApiSchema>
                        {
                            ["employeesFile"] = new OpenApiSchema
                            {
                                Type = JsonSchemaType.String,
                                Format = "binary"
                            }
                        }
                    }
                }
            }
        };
    }

    private static OpenApiSchema CreateJsonSchema()
        => new()
        {
            Type = JsonSchemaType.Array,
            Items = new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Required = new HashSet<string> { "name", "email", "tel", "joined" },
                Properties = new Dictionary<string, IOpenApiSchema>
                {
                    ["name"] = new OpenApiSchema { Type = JsonSchemaType.String },
                    ["email"] = new OpenApiSchema { Type = JsonSchemaType.String },
                    ["tel"] = new OpenApiSchema { Type = JsonSchemaType.String },
                    ["joined"] = new OpenApiSchema { Type = JsonSchemaType.String, Format = "date" }
                }
            }
        };

    private static OpenApiSchema CreatePlainTextSchema()
        => new()
        {
            Type = JsonSchemaType.String
        };
}
