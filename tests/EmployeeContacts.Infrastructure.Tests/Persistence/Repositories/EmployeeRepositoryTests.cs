namespace EmployeeContacts.Infrastructure.Tests.Persistence.Repositories;

public class EmployeeRepositoryTests
{
    [Fact(DisplayName = "GetPagedAsync는 Name, Id 오름차순 정렬을 보장한다.")]
    public async Task GetPagedAsync_ShouldReturnEmployees_OrderedByNameThenId()
    {
        await using InfrastructureTestHost host = InfrastructureTestHost.Create();
        await host.MigrateAsync();

        IEmployeeRepository repository = host.GetEmployeeRepository();
        IUnitOfWork unitOfWork = host.GetUnitOfWork();

        Employee kimB = EmployeeTestData.CreateEmployee(
            Guid.Parse("00000000-0000-0000-0000-000000000002"),
            "김철수",
            "kim-b@example.com",
            "01012345678",
            "2024-02-01");
        Employee park = EmployeeTestData.CreateEmployee(
            Guid.Parse("00000000-0000-0000-0000-000000000003"),
            "박영희",
            "park@example.com",
            "01087654321",
            "2024-02-02");
        Employee kimA = EmployeeTestData.CreateEmployee(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            "김철수",
            "kim-a@example.com",
            "01011112222",
            "2024-02-03");

        await repository.AddRangeAsync([kimB, park, kimA], CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        PagedResult<EmployeeDto> result = await repository.GetPagedAsync(1, 10, CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(["kim-a@example.com", "kim-b@example.com", "park@example.com"], result.Items.Select(item => item.Email).ToArray());
    }

    [Fact(DisplayName = "GetByNameAsync는 exact match로 조회한다.")]
    public async Task GetByNameAsync_ShouldReturnEmployees_WithExactNameMatch()
    {
        await using InfrastructureTestHost host = InfrastructureTestHost.Create();
        await host.MigrateAsync();

        IEmployeeRepository repository = host.GetEmployeeRepository();
        IUnitOfWork unitOfWork = host.GetUnitOfWork();

        await repository.AddRangeAsync(
        [
            EmployeeTestData.CreateEmployee(Guid.NewGuid(), "김철수", "kim@example.com", "01012345678", "2024-02-01"),
            EmployeeTestData.CreateEmployee(Guid.NewGuid(), "김철수2", "kim2@example.com", "01087654321", "2024-02-02")
        ], CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        IReadOnlyList<EmployeeDto> result = await repository.GetByNameAsync("김철수", CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("김철수", result[0].Name);
        Assert.Equal("kim@example.com", result[0].Email);
    }

    [Fact(DisplayName = "GetExistingEmailsAsync는 전달한 값과 일치하는 이메일만 반환한다.")]
    public async Task GetExistingEmailsAsync_ShouldReturnMatchingEmailsOnly()
    {
        await using InfrastructureTestHost host = InfrastructureTestHost.Create();
        await host.MigrateAsync();

        IEmployeeRepository repository = host.GetEmployeeRepository();
        IUnitOfWork unitOfWork = host.GetUnitOfWork();

        await repository.AddRangeAsync(
        [
            EmployeeTestData.CreateEmployee(Guid.NewGuid(), "김철수", "kim@example.com", "01012345678", "2024-02-01"),
            EmployeeTestData.CreateEmployee(Guid.NewGuid(), "박영희", "park@example.com", "01087654321", "2024-02-02")
        ], CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        IReadOnlySet<string> result = await repository.GetExistingEmailsAsync(
            ["kim@example.com", "missing@example.com"],
            CancellationToken.None);

        Assert.Single(result);
        Assert.Contains("kim@example.com", result);
    }

    [Fact(DisplayName = "GetExistingPhoneNumbersAsync는 전달한 값과 일치하는 전화번호만 반환한다.")]
    public async Task GetExistingPhoneNumbersAsync_ShouldReturnMatchingPhoneNumbersOnly()
    {
        await using InfrastructureTestHost host = InfrastructureTestHost.Create();
        await host.MigrateAsync();

        IEmployeeRepository repository = host.GetEmployeeRepository();
        IUnitOfWork unitOfWork = host.GetUnitOfWork();

        await repository.AddRangeAsync(
        [
            EmployeeTestData.CreateEmployee(Guid.NewGuid(), "김철수", "kim@example.com", "01012345678", "2024-02-01"),
            EmployeeTestData.CreateEmployee(Guid.NewGuid(), "박영희", "park@example.com", "01087654321", "2024-02-02")
        ], CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        IReadOnlySet<string> result = await repository.GetExistingPhoneNumbersAsync(
            ["01087654321", "01000000000"],
            CancellationToken.None);

        Assert.Single(result);
        Assert.Contains("01087654321", result);
    }

    [Fact(DisplayName = "빈 이메일 조회 입력은 DB 조회 없이 빈 집합을 반환한다.")]
    public async Task GetExistingEmailsAsync_ShouldReturnEmptySet_WhenInputIsEmpty()
    {
        await using InfrastructureTestHost host = InfrastructureTestHost.Create();
        await host.MigrateAsync();

        IEmployeeRepository repository = host.GetEmployeeRepository();

        IReadOnlySet<string> result = await repository.GetExistingEmailsAsync([], CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "빈 전화번호 조회 입력은 DB 조회 없이 빈 집합을 반환한다.")]
    public async Task GetExistingPhoneNumbersAsync_ShouldReturnEmptySet_WhenInputIsEmpty()
    {
        await using InfrastructureTestHost host = InfrastructureTestHost.Create();
        await host.MigrateAsync();

        IEmployeeRepository repository = host.GetEmployeeRepository();

        IReadOnlySet<string> result = await repository.GetExistingPhoneNumbersAsync([], CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "AddRangeAsync와 SaveChangesAsync 호출 후 직원이 실제 저장된다.")]
    public async Task SaveChangesAsync_ShouldPersistEmployees_AddedByRepository()
    {
        await using InfrastructureTestHost host = InfrastructureTestHost.Create();
        await host.MigrateAsync();

        IEmployeeRepository repository = host.GetEmployeeRepository();
        IUnitOfWork unitOfWork = host.GetUnitOfWork();

        await repository.AddRangeAsync(
        [
            EmployeeTestData.CreateEmployee(Guid.NewGuid(), "김철수", "kim@example.com", "01012345678", "2024-02-01")
        ], CancellationToken.None);

        PagedResult<EmployeeDto> beforeSave = await repository.GetPagedAsync(1, 10, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);
        PagedResult<EmployeeDto> afterSave = await repository.GetPagedAsync(1, 10, CancellationToken.None);

        Assert.Empty(beforeSave.Items);
        Assert.Single(afterSave.Items);
        Assert.Equal("01012345678", afterSave.Items[0].Tel);
    }
}
