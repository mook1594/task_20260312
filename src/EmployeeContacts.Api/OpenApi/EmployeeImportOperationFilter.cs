using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

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

        // 201 Created 응답 예제
        if (operation.Responses.ContainsKey("201"))
        {
            operation.Responses["201"].Content["application/json"] = new()
            {
                Schema = new OpenApiSchema { Type = JsonSchemaType.Object },
                Example = JsonNode.Parse("""
                {
                  "total": 3,
                  "created": 2,
                  "failed": 1,
                  "errors": [
                    {
                      "row": 2,
                      "field": "email",
                      "code": "InvalidEmail",
                      "message": "Email format is invalid."
                    }
                  ]
                }
                """)
            };
        }

        // 400 Bad Request 응답 예제
        if (operation.Responses.ContainsKey("400"))
        {
            operation.Responses["400"].Content["application/problem+json"] = new()
            {
                Schema = new OpenApiSchema { Type = JsonSchemaType.Object },
                Example = JsonNode.Parse("""
                {
                  "type": "https://httpstatuses.com/400",
                  "title": "One or more validation errors occurred.",
                  "status": 400,
                  "errors": [
                    {
                      "row": 1,
                      "message": "Invalid JSON format"
                    },
                    {
                      "row": 2,
                      "message": "Phone number format is invalid. Accepted format: 010-1234-5678"
                    }
                  ],
                  "traceId": "0hmv4guvgi1qqemd48p1cfrkq0"
                }
                """)
            };
        }

        // 415 Unsupported Media Type 응답 예제
        if (operation.Responses.ContainsKey("415"))
        {
            operation.Responses["415"].Content["application/problem+json"] = new()
            {
                Schema = new OpenApiSchema { Type = JsonSchemaType.Object },
                Example = JsonNode.Parse("""
                {
                  "type": "https://httpstatuses.com/415",
                  "title": "Unsupported Media Type",
                  "status": 415,
                  "detail": "Content-Type header is not supported. Supported types: application/json, text/csv, text/plain, multipart/form-data",
                  "traceId": "0hmv4guvgi1qqemd48p1cfrkq0"
                }
                """)
            };
        }

        // 500 Internal Server Error 응답 예제
        if (operation.Responses.ContainsKey("500"))
        {
            operation.Responses["500"].Content["application/problem+json"] = new()
            {
                Schema = new OpenApiSchema { Type = JsonSchemaType.Object },
                Example = JsonNode.Parse("""
                {
                  "type": "https://httpstatuses.com/500",
                  "title": "Internal Server Error",
                  "status": 500,
                  "detail": "An unexpected error occurred.",
                  "traceId": "0hmv4guvgi1qqemd48p1cfrkq0"
                }
                """)
            };
        }
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
