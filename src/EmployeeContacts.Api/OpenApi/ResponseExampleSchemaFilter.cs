using EmployeeContacts.Api.Models;
using EmployeeContacts.Application.Employees.Commands.BulkCreateEmployees;
using EmployeeContacts.Application.Employees.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace EmployeeContacts.Api.OpenApi;

/// <summary>
/// Response 모델에 샘플 데이터를 추가하는 SchemaFilter다.
/// </summary>
public sealed class ResponseExampleSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(context);

        if (schema is not OpenApiSchema openApiSchema)
        {
            return;
        }

        var type = context.Type;

        // EmployeeDto 샘플 데이터
        if (type == typeof(EmployeeDto))
        {
            openApiSchema.Example = JsonNode.Parse("""
            {
              "id": "550e8400-e29b-41d4-a716-446655440000",
              "name": "김철수",
              "email": "kim.chulsu@example.com",
              "tel": "01012345678",
              "joined": "2024-02-15"
            }
            """);
        }

        // PagedResponse<EmployeeDto> 샘플 데이터
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(PagedResponse<>))
        {
            if (type.GenericTypeArguments[0] == typeof(EmployeeDto))
            {
                openApiSchema.Example = JsonNode.Parse("""
                {
                  "items": [
                    {
                      "id": "550e8400-e29b-41d4-a716-446655440000",
                      "name": "김철수",
                      "email": "kim.chulsu@example.com",
                      "tel": "01012345678",
                      "joined": "2024-02-15"
                    },
                    {
                      "id": "550e8400-e29b-41d4-a716-446655440001",
                      "name": "이영희",
                      "email": "lee.younghee@example.com",
                      "tel": "01087654321",
                      "joined": "2024-03-10"
                    }
                  ],
                  "page": 1,
                  "pageSize": 20,
                  "totalCount": 25,
                  "totalPages": 2,
                  "links": {
                    "next": "/api/employee?page=2&pageSize=20",
                    "prev": null
                  }
                }
                """);
            }
        }

        // IReadOnlyList<EmployeeDto> 샘플 데이터
        if (type == typeof(IReadOnlyList<EmployeeDto>) ||
            (type.IsGenericType &&
             type.GetGenericTypeDefinition() == typeof(List<>) &&
             type.GenericTypeArguments[0] == typeof(EmployeeDto)))
        {
            openApiSchema.Example = JsonNode.Parse("""
            [
              {
                "id": "550e8400-e29b-41d4-a716-446655440000",
                "name": "김철수",
                "email": "kim.chulsu@example.com",
                "tel": "01012345678",
                "joined": "2024-02-15"
              }
            ]
            """);
        }

        // BulkCreateEmployeesResult 샘플 데이터
        if (type == typeof(BulkCreateEmployeesResult))
        {
            openApiSchema.Example = JsonNode.Parse("""
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
            """);
        }

        // ValidationProblemDetails 샘플 데이터
        if (type == typeof(ValidationProblemDetails))
        {
            openApiSchema.Example = JsonNode.Parse("""
            {
              "type": "https://httpstatuses.com/400",
              "title": "One or more validation errors occurred.",
              "status": 400,
              "errors": {
                "page": [
                  "page는 1 이상이어야 합니다."
                ],
                "pageSize": [
                  "pageSize는 1부터 100 사이여야 합니다."
                ]
              },
              "traceId": "0hmv4guvgi1qqemd48p1cfrkq0"
            }
            """);
        }

        // ProblemDetails 샘플 데이터 - 400 Bad Request
        if (type == typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))
        {
            // context.ParameterInfo를 통해 더 구체적인 구분이 가능하지만,
            // 여기서는 가장 일반적인 형태를 제공
            openApiSchema.Example = JsonNode.Parse("""
            {
              "type": "https://httpstatuses.com/400",
              "title": "Bad Request",
              "status": 400,
              "detail": "The request conflicts with existing employee data.",
              "traceId": "0hmv4guvgi1qqemd48p1cfrkq0"
            }
            """);
        }
    }
}
