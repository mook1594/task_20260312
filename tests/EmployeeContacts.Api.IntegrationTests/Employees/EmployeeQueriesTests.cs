using EmployeeContacts.Api.IntegrationTests.TestCommon;

namespace EmployeeContacts.Api.IntegrationTests.Employees;

public sealed class EmployeeQueriesTests
{
    [Fact(DisplayName = "직원 목록 정상 조회 시 페이지 결과를 반환한다.")]
    public async Task GetEmployees_ShouldReturnPagedEmployees()
    {
        using EmployeeContactsApiFactory factory = new();
        await factory.SeedEmployeesAsync(
                CreateEmployee("김철수", "kim@example.com", "01012345678", new Guid("00000000-0000-0000-0000-000000000002")),
                CreateEmployee("박영희", "park@example.com", "01087654321", new Guid("00000000-0000-0000-0000-000000000003")))
            .ConfigureAwait(false);
        using HttpClient client = factory.CreateApiClient();

        PagedResult<EmployeeDto>? response = await client
            .GetFromJsonAsync<PagedResult<EmployeeDto>>("/api/employee")
            .ConfigureAwait(false);

        Assert.NotNull(response);
        Assert.Equal(1, response.Page);
        Assert.Equal(20, response.PageSize);
        Assert.Equal(2, response.TotalCount);
        Assert.Equal(1, response.TotalPages);
        Assert.Equal(["김철수", "박영희"], response.Items.Select(item => item.Name).ToArray());
    }

    [Fact(DisplayName = "잘못된 page, pageSize는 ValidationProblemDetails를 반환한다.")]
    public async Task GetEmployees_ShouldReturnValidationProblem_WhenPagingIsInvalid()
    {
        using EmployeeContactsApiFactory factory = new();
        using HttpClient client = factory.CreateApiClient();

        using HttpResponseMessage response = await client.GetAsync("/api/employee?page=0&pageSize=101").ConfigureAwait(false);
        ValidationProblemDetails? problemDetails = await response.Content
            .ReadFromJsonAsync<ValidationProblemDetails>()
            .ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemDetails);
        Assert.Contains("page", problemDetails.Errors.Keys);
        Assert.Contains("pageSize", problemDetails.Errors.Keys);
        Assert.Contains("traceId", problemDetails.Extensions.Keys);
    }

    [Fact(DisplayName = "직원 목록은 name, id 오름차순으로 정렬된다.")]
    public async Task GetEmployees_ShouldReturnEmployees_OrderedByNameThenId()
    {
        using EmployeeContactsApiFactory factory = new();
        await factory.SeedEmployeesAsync(
                CreateEmployee("박영희", "park@example.com", "01087654321", new Guid("00000000-0000-0000-0000-000000000003")),
                CreateEmployee("김철수", "kim-b@example.com", "01012345679", new Guid("00000000-0000-0000-0000-000000000005")),
                CreateEmployee("김철수", "kim-a@example.com", "01012345678", new Guid("00000000-0000-0000-0000-000000000004")))
            .ConfigureAwait(false);
        using HttpClient client = factory.CreateApiClient();

        PagedResult<EmployeeDto>? response = await client
            .GetFromJsonAsync<PagedResult<EmployeeDto>>("/api/employee?page=1&pageSize=10")
            .ConfigureAwait(false);

        Assert.NotNull(response);
        Assert.Equal(
            ["kim-a@example.com", "kim-b@example.com", "park@example.com"],
            response.Items.Select(item => item.Email).ToArray());
    }

    [Fact(DisplayName = "이름 검색은 trim 후 exact match로 동작한다.")]
    public async Task GetEmployeesByName_ShouldTrimNameBeforeSearching()
    {
        using EmployeeContactsApiFactory factory = new();
        await factory.SeedEmployeesAsync(
                CreateEmployee("김철수", "kim@example.com", "01012345678"),
                CreateEmployee("김철수2", "kim2@example.com", "01012345679"))
            .ConfigureAwait(false);
        using HttpClient client = factory.CreateApiClient();

        IReadOnlyList<EmployeeDto>? response = await client
            .GetFromJsonAsync<IReadOnlyList<EmployeeDto>>($"/api/employee/{Uri.EscapeDataString("  김철수  ")}")
            .ConfigureAwait(false);

        Assert.NotNull(response);
        Assert.Single(response);
        Assert.Equal("김철수", response[0].Name);
    }

    [Fact(DisplayName = "이름 검색 결과가 없으면 빈 배열을 반환한다.")]
    public async Task GetEmployeesByName_ShouldReturnEmptyArray_WhenNoEmployeeExists()
    {
        using EmployeeContactsApiFactory factory = new();
        using HttpClient client = factory.CreateApiClient();

        IReadOnlyList<EmployeeDto>? response = await client
            .GetFromJsonAsync<IReadOnlyList<EmployeeDto>>($"/api/employee/{Uri.EscapeDataString("없는사람")}")
            .ConfigureAwait(false);

        Assert.NotNull(response);
        Assert.Empty(response);
    }

    [Fact(DisplayName = "공백 이름은 ValidationProblemDetails를 반환한다.")]
    public async Task GetEmployeesByName_ShouldReturnValidationProblem_WhenNameIsEmptyAfterTrim()
    {
        using EmployeeContactsApiFactory factory = new();
        using HttpClient client = factory.CreateApiClient();

        using HttpResponseMessage response = await client
            .GetAsync($"/api/employee/{Uri.EscapeDataString("   ")}")
            .ConfigureAwait(false);
        ValidationProblemDetails? problemDetails = await response.Content
            .ReadFromJsonAsync<ValidationProblemDetails>()
            .ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemDetails);
        Assert.Contains("name", problemDetails.Errors.Keys);
    }

    private static EmployeeEntity CreateEmployee(
        string name,
        string email,
        string phoneNumber,
        Guid? id = null)
        => new()
        {
            Id = id ?? Guid.CreateVersion7(),
            Name = name,
            Email = email,
            PhoneNumber = phoneNumber,
            Joined = new DateOnly(2024, 2, 1),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}
