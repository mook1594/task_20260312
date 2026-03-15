using EmployeeContacts.Api.IntegrationTests.TestCommon;
using EmployeeContacts.Api.Models;

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

        PagedResponse<EmployeeDto>? response = await client
            .GetFromJsonAsync<PagedResponse<EmployeeDto>>("/api/employee")
            .ConfigureAwait(false);

        Assert.NotNull(response);
        Assert.Equal(1, response.Page);
        Assert.Equal(20, response.PageSize);
        Assert.Equal(2, response.TotalCount);
        Assert.Equal(1, response.TotalPages);
        Assert.Equal(["김철수", "박영희"], response.Items.Select(item => item.Name).ToArray());
        Assert.NotNull(response.Links);
        Assert.Null(response.Links.Next);
        Assert.Null(response.Links.Prev);
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

        PagedResponse<EmployeeDto>? response = await client
            .GetFromJsonAsync<PagedResponse<EmployeeDto>>("/api/employee?page=1&pageSize=10")
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

    [Fact(DisplayName = "페이지네이션 링크는 다음/이전 페이지 URL을 포함한다.")]
    public async Task GetEmployees_ShouldIncludePaginationLinks()
    {
        using EmployeeContactsApiFactory factory = new();
        await factory.SeedEmployeesAsync(
                CreateEmployee("직원1", "emp1@example.com", "01000000001", new Guid("00000000-0000-0000-0000-000000000001")),
                CreateEmployee("직원2", "emp2@example.com", "01000000002", new Guid("00000000-0000-0000-0000-000000000002")),
                CreateEmployee("직원3", "emp3@example.com", "01000000003", new Guid("00000000-0000-0000-0000-000000000003")),
                CreateEmployee("직원4", "emp4@example.com", "01000000004", new Guid("00000000-0000-0000-0000-000000000004")),
                CreateEmployee("직원5", "emp5@example.com", "01000000005", new Guid("00000000-0000-0000-0000-000000000005")),
                CreateEmployee("직원6", "emp6@example.com", "01000000006", new Guid("00000000-0000-0000-0000-000000000006")))
            .ConfigureAwait(false);
        using HttpClient client = factory.CreateApiClient();

        PagedResponse<EmployeeDto>? firstPage = await client
            .GetFromJsonAsync<PagedResponse<EmployeeDto>>("/api/employee?page=1&pageSize=2")
            .ConfigureAwait(false);

        Assert.NotNull(firstPage);
        Assert.Equal(1, firstPage.Page);
        Assert.Equal(2, firstPage.Items.Count);
        Assert.NotNull(firstPage.Links);
        Assert.Null(firstPage.Links.Prev);
        Assert.NotNull(firstPage.Links.Next);
        Assert.Contains("page=2", firstPage.Links.Next);
        Assert.Contains("pageSize=2", firstPage.Links.Next);

        PagedResponse<EmployeeDto>? secondPage = await client
            .GetFromJsonAsync<PagedResponse<EmployeeDto>>("/api/employee?page=2&pageSize=2")
            .ConfigureAwait(false);

        Assert.NotNull(secondPage);
        Assert.Equal(2, secondPage.Page);
        Assert.NotNull(secondPage.Links);
        Assert.NotNull(secondPage.Links.Prev);
        Assert.NotNull(secondPage.Links.Next);
        Assert.Contains("page=1", secondPage.Links.Prev);
        Assert.Contains("page=3", secondPage.Links.Next);

        PagedResponse<EmployeeDto>? lastPage = await client
            .GetFromJsonAsync<PagedResponse<EmployeeDto>>("/api/employee?page=3&pageSize=2")
            .ConfigureAwait(false);

        Assert.NotNull(lastPage);
        Assert.Equal(3, lastPage.Page);
        Assert.NotNull(lastPage.Links);
        Assert.NotNull(lastPage.Links.Prev);
        Assert.Null(lastPage.Links.Next);
        Assert.Contains("page=2", lastPage.Links.Prev);
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
