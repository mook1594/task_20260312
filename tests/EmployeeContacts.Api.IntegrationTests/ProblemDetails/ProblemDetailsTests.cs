using EmployeeContacts.Api.IntegrationTests.TestCommon;
using EmployeeContacts.Application.Abstractions.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EmployeeContacts.Api.IntegrationTests.ProblemDetails;

public sealed class ProblemDetailsTests
{
    [Fact(DisplayName = "ValidationProblemDetails에 입력 키와 오류 메시지가 포함된다.")]
    public async Task ValidationProblem_ShouldContainErrorDictionary()
    {
        using EmployeeContactsApiFactory factory = new();
        using HttpClient client = factory.CreateApiClient();

        using HttpResponseMessage response = await client.GetAsync("/api/employee?page=0").ConfigureAwait(false);
        ValidationProblemDetails? problemDetails = await response.Content
            .ReadFromJsonAsync<ValidationProblemDetails>()
            .ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemDetails);
        Assert.Contains("page", problemDetails.Errors.Keys);
        Assert.NotEmpty(problemDetails.Errors["page"]);
    }

    [Fact(DisplayName = "ProblemDetails에 traceId가 포함된다.")]
    public async Task ProblemDetails_ShouldContainTraceId()
    {
        using EmployeeContactsApiFactory factory = new();
        using HttpClient client = factory.CreateApiClient();
        using StringContent content = new("{}", Encoding.UTF8, "application/xml");

        using HttpResponseMessage response = await client.PostAsync("/api/employee", content).ConfigureAwait(false);
        MvcProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<MvcProblemDetails>().ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        Assert.NotNull(problemDetails);
        Assert.True(problemDetails.Extensions.ContainsKey("traceId"));
    }

    [Fact(DisplayName = "예기치 않은 예외는 500 ProblemDetails로 변환된다.")]
    public async Task UnhandledException_ShouldReturnInternalServerErrorProblemDetails()
    {
        using EmployeeContactsApiFactory factory = new(services =>
        {
            services.RemoveAll<IPlainTextEmployeeImportDetector>();
            services.AddSingleton<IPlainTextEmployeeImportDetector, ThrowingPlainTextEmployeeImportDetector>();
        });
        using HttpClient client = factory.CreateApiClient();
        using StringContent content = new("김철수,kim@example.com,01012345678,2024-02-01", Encoding.UTF8, "text/plain");

        using HttpResponseMessage response = await client.PostAsync("/api/employee", content).ConfigureAwait(false);
        MvcProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<MvcProblemDetails>().ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.NotNull(problemDetails);
        Assert.Equal("Internal Server Error", problemDetails.Title);
        Assert.True(problemDetails.Extensions.ContainsKey("traceId"));
    }
}
