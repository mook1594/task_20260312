using EmployeeContacts.Api.IntegrationTests.TestCommon;

namespace EmployeeContacts.Api.IntegrationTests.Employees;

public sealed class EmployeeImportTests
{
    [Fact(DisplayName = "multipart/form-data CSV 업로드로 직원을 등록한다.")]
    public async Task BulkCreateEmployees_ShouldCreateEmployees_FromMultipartCsv()
    {
        using EmployeeContactsApiFactory factory = new();
        using HttpClient client = factory.CreateApiClient();
        using MultipartFormDataContent content = new();
        content.Add(
            new ByteArrayContent(Encoding.UTF8.GetBytes("김철수,kim@example.com,01012345678,2024-02-01")),
            "employeesFile",
            "employees.csv");

        using HttpResponseMessage response = await client.PostAsync("/api/employee", content).ConfigureAwait(false);
        BulkCreateEmployeesResult? result = await response.Content
            .ReadFromJsonAsync<BulkCreateEmployeesResult>()
            .ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(1, result.Created);
        Assert.Null(response.Headers.Location);
    }

    [Fact(DisplayName = "text/csv 본문으로 직원을 등록한다.")]
    public async Task BulkCreateEmployees_ShouldCreateEmployees_FromCsvBody()
    {
        using EmployeeContactsApiFactory factory = new();
        using HttpClient client = factory.CreateApiClient();
        using StringContent content = new("김철수,kim@example.com,01012345678,2024-02-01", Encoding.UTF8, "text/csv");

        using HttpResponseMessage response = await client.PostAsync("/api/employee", content).ConfigureAwait(false);
        BulkCreateEmployeesResult? result = await response.Content
            .ReadFromJsonAsync<BulkCreateEmployeesResult>()
            .ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(1, result.Created);
    }

    [Fact(DisplayName = "application/json 배열로 직원을 등록한다.")]
    public async Task BulkCreateEmployees_ShouldCreateEmployees_FromJsonArray()
    {
        using EmployeeContactsApiFactory factory = new();
        using HttpClient client = factory.CreateApiClient();
        using JsonContent content = JsonContent.Create(new[]
        {
            new { name = "김철수", email = "kim@example.com", tel = "010-1234-5678", joined = "2024-02-01" }
        });

        using HttpResponseMessage response = await client.PostAsync("/api/employee", content).ConfigureAwait(false);
        BulkCreateEmployeesResult? result = await response.Content
            .ReadFromJsonAsync<BulkCreateEmployeesResult>()
            .ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(1, result.Created);
    }

    [Fact(DisplayName = "text/plain이 JSON 배열이면 JSON parser를 우선 선택한다.")]
    public async Task BulkCreateEmployees_ShouldPreferJsonParser_ForPlainTextJsonArray()
    {
        using EmployeeContactsApiFactory factory = new();
        using HttpClient client = factory.CreateApiClient();
        using StringContent content = new(
            "[{\"name\":\"김철수\",\"email\":\"kim@example.com\",\"tel\":\"010-1234-5678\",\"joined\":\"2024-02-01\"}]",
            Encoding.UTF8,
            "text/plain");

        using HttpResponseMessage response = await client.PostAsync("/api/employee", content).ConfigureAwait(false);
        BulkCreateEmployeesResult? result = await response.Content
            .ReadFromJsonAsync<BulkCreateEmployeesResult>()
            .ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(1, result.Created);
    }

    [Fact(DisplayName = "text/plain이 JSON 배열이 아니면 CSV parser로 fallback 한다.")]
    public async Task BulkCreateEmployees_ShouldFallbackToCsvParser_ForPlainTextCsv()
    {
        using EmployeeContactsApiFactory factory = new();
        using HttpClient client = factory.CreateApiClient();
        using StringContent content = new("김철수,kim@example.com,01012345678,2024-02-01", Encoding.UTF8, "text/plain");

        using HttpResponseMessage response = await client.PostAsync("/api/employee", content).ConfigureAwait(false);
        BulkCreateEmployeesResult? result = await response.Content
            .ReadFromJsonAsync<BulkCreateEmployeesResult>()
            .ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(1, result.Created);
    }

    [Fact(DisplayName = "일부 행만 성공해도 201과 결과 집계를 반환한다.")]
    public async Task BulkCreateEmployees_ShouldReturnPartialSuccess_WhenSomeRowsFail()
    {
        using EmployeeContactsApiFactory factory = new();
        using HttpClient client = factory.CreateApiClient();
        using JsonContent content = JsonContent.Create(new object[]
        {
            new { name = "김철수", email = "kim@example.com", tel = "010-1234-5678", joined = "2024-02-01" },
            new { name = "박영희", email = "kim@example.com", tel = "010-8765-4321", joined = "2024-02-02" }
        });

        using HttpResponseMessage response = await client.PostAsync("/api/employee", content).ConfigureAwait(false);
        BulkCreateEmployeesResult? result = await response.Content
            .ReadFromJsonAsync<BulkCreateEmployeesResult>()
            .ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(2, result.Total);
        Assert.Equal(1, result.Created);
        Assert.Equal(1, result.Failed);
        Assert.Single(result.Errors);
        Assert.Equal(2, result.Errors[0].Row);
        Assert.Equal("email", result.Errors[0].Field);
    }

    [Fact(DisplayName = "헤더가 있는 CSV에서도 errors.row는 데이터 행 기준이다.")]
    public async Task BulkCreateEmployees_ShouldReturnRowNumbers_FromDataRows_WhenCsvHasHeader()
    {
        using EmployeeContactsApiFactory factory = new();
        using HttpClient client = factory.CreateApiClient();
        using StringContent content = new(
            "name,email,tel,joined\n김철수,kim@example.com,01012345678,2024-02-01\n박영희,kim@example.com,01087654321,2024-02-02",
            Encoding.UTF8,
            "text/csv");

        using HttpResponseMessage response = await client.PostAsync("/api/employee", content).ConfigureAwait(false);
        BulkCreateEmployeesResult? result = await response.Content
            .ReadFromJsonAsync<BulkCreateEmployeesResult>()
            .ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.Single(result.Errors);
        Assert.Equal(2, result.Errors[0].Row);
    }

    [Fact(DisplayName = "지원하지 않는 Content-Type은 415를 반환한다.")]
    public async Task BulkCreateEmployees_ShouldReturnUnsupportedMediaType_WhenContentTypeIsNotSupported()
    {
        using EmployeeContactsApiFactory factory = new();
        using HttpClient client = factory.CreateApiClient();
        using StringContent content = new("{}", Encoding.UTF8, "application/xml");

        using HttpResponseMessage response = await client.PostAsync("/api/employee", content).ConfigureAwait(false);
        MvcProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<MvcProblemDetails>().ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        Assert.NotNull(problemDetails);
        Assert.Contains("traceId", problemDetails.Extensions.Keys);
    }

    [Fact(DisplayName = "잘못된 포맷은 400 ProblemDetails를 반환한다.")]
    public async Task BulkCreateEmployees_ShouldReturnProblemDetails_WhenFormatIsInvalid()
    {
        using EmployeeContactsApiFactory factory = new();
        using HttpClient client = factory.CreateApiClient();
        using StringContent content = new("{\"name\":\"김철수\"}", Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await client.PostAsync("/api/employee", content).ConfigureAwait(false);
        MvcProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<MvcProblemDetails>().ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemDetails);
        Assert.Equal("Bad Request", problemDetails.Title);
    }

    [Fact(DisplayName = "한 건도 생성되지 않으면 400 ProblemDetails를 반환한다.")]
    public async Task BulkCreateEmployees_ShouldReturnProblemDetails_WhenNothingIsCreated()
    {
        using EmployeeContactsApiFactory factory = new();
        using HttpClient client = factory.CreateApiClient();
        using JsonContent content = JsonContent.Create(new[]
        {
            new { name = "", email = "invalid", tel = "010", joined = "2024/02/01" }
        });

        using HttpResponseMessage response = await client.PostAsync("/api/employee", content).ConfigureAwait(false);
        MvcProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<MvcProblemDetails>().ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemDetails);
        Assert.Contains("traceId", problemDetails.Extensions.Keys);
    }

    [Fact(DisplayName = "multipart/form-data 파일 파트 누락은 400 ProblemDetails를 반환한다.")]
    public async Task BulkCreateEmployees_ShouldReturnProblemDetails_WhenMultipartFileIsMissing()
    {
        using EmployeeContactsApiFactory factory = new();
        using HttpClient client = factory.CreateApiClient();
        using MultipartFormDataContent content = new();

        using HttpResponseMessage response = await client.PostAsync("/api/employee", content).ConfigureAwait(false);
        MvcProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<MvcProblemDetails>().ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemDetails);
    }
}
