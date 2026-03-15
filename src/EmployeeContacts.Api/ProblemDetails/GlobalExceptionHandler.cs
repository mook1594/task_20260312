using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EmployeeContacts.Api.ProblemDetails;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        (int statusCode, Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails) = MapException(httpContext, exception);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception while processing {RequestMethod} {RequestPath}.",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(
                exception,
                "Request failed with status code {StatusCode} for {RequestMethod} {RequestPath}.",
                statusCode,
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await JsonSerializer.SerializeAsync(
                httpContext.Response.Body,
                problemDetails,
                problemDetails.GetType(),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return true;
    }

    private static (int StatusCode, Microsoft.AspNetCore.Mvc.ProblemDetails ProblemDetails) MapException(
        HttpContext httpContext,
        Exception exception)
    {
        if (exception is HttpProblemException httpProblemException)
        {
            return (httpProblemException.StatusCode, CreateProblemDetails(
                httpContext,
                httpProblemException.StatusCode,
                httpProblemException.Title,
                httpProblemException.Detail));
        }

        if (exception is ValidationException validationException)
        {
            ValidationProblemDetails validationProblemDetails = new(
                validationException.Errors
                    .GroupBy(error => ToCamelCase(error.PropertyName), StringComparer.Ordinal)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).Distinct(StringComparer.Ordinal).ToArray(),
                        StringComparer.Ordinal))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
                Type = "https://httpstatuses.com/400"
            };

            validationProblemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
            return (StatusCodes.Status400BadRequest, validationProblemDetails);
        }

        if (exception is EmployeeContacts.Application.Common.Errors.ApplicationException applicationException)
        {
            Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails = CreateProblemDetails(
                httpContext,
                StatusCodes.Status400BadRequest,
                "Bad Request",
                applicationException.Detail);
            problemDetails.Extensions["code"] = applicationException.Code;

            return (StatusCodes.Status400BadRequest, problemDetails);
        }

        if (IsUniqueConstraintViolation(exception))
        {
            return (StatusCodes.Status400BadRequest, CreateProblemDetails(
                httpContext,
                StatusCodes.Status400BadRequest,
                "Bad Request",
                "The request conflicts with existing employee data."));
        }

        return (StatusCodes.Status500InternalServerError, CreateProblemDetails(
            httpContext,
            StatusCodes.Status500InternalServerError,
            "Internal Server Error",
            "An unexpected error occurred."));
    }

    private static Microsoft.AspNetCore.Mvc.ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        int statusCode,
        string title,
        string detail)
    {
        Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails = new()
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        return problemDetails;
    }

    private static bool IsUniqueConstraintViolation(Exception exception)
        => exception is DbUpdateException dbUpdateException
           && dbUpdateException.InnerException is SqliteException sqliteException
           && sqliteException.SqliteErrorCode == 19;

    private static string ToCamelCase(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return string.Empty;
        }

        string segment = propertyName.Split('.').Last();
        if (segment.Length == 1)
        {
            return segment.ToLowerInvariant();
        }

        return char.ToLowerInvariant(segment[0]) + segment[1..];
    }
}
