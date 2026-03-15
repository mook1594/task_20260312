namespace EmployeeContacts.Infrastructure.Tests.Persistence.UnitOfWork;

public class EfUnitOfWorkTests
{
    [Fact(DisplayName = "SaveChangesAsync는 실제 커밋 경계로 동작한다.")]
    public async Task SaveChangesAsync_ShouldCommitPendingRepositoryChanges()
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
    }

    [Fact(DisplayName = "이메일 유니크 인덱스 충돌은 저장 시점 예외로 전파된다.")]
    public async Task SaveChangesAsync_ShouldThrow_WhenEmailUniqueIndexIsViolated()
    {
        await using InfrastructureTestHost host = InfrastructureTestHost.Create();
        await host.MigrateAsync();

        IEmployeeRepository repository = host.GetEmployeeRepository();
        IUnitOfWork unitOfWork = host.GetUnitOfWork();

        await repository.AddRangeAsync(
        [
            EmployeeTestData.CreateEmployee(Guid.NewGuid(), "김철수", "kim@example.com", "01012345678", "2024-02-01")
        ], CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        await repository.AddRangeAsync(
        [
            EmployeeTestData.CreateEmployee(Guid.NewGuid(), "박영희", "kim@example.com", "01087654321", "2024-02-02")
        ], CancellationToken.None);

        await Assert.ThrowsAsync<DbUpdateException>(() => unitOfWork.SaveChangesAsync(CancellationToken.None));
    }

    [Fact(DisplayName = "전화번호 유니크 인덱스 충돌은 저장 시점 예외로 전파된다.")]
    public async Task SaveChangesAsync_ShouldThrow_WhenPhoneNumberUniqueIndexIsViolated()
    {
        await using InfrastructureTestHost host = InfrastructureTestHost.Create();
        await host.MigrateAsync();

        IEmployeeRepository repository = host.GetEmployeeRepository();
        IUnitOfWork unitOfWork = host.GetUnitOfWork();

        await repository.AddRangeAsync(
        [
            EmployeeTestData.CreateEmployee(Guid.NewGuid(), "김철수", "kim@example.com", "01012345678", "2024-02-01")
        ], CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        await repository.AddRangeAsync(
        [
            EmployeeTestData.CreateEmployee(Guid.NewGuid(), "박영희", "park@example.com", "01012345678", "2024-02-02")
        ], CancellationToken.None);

        await Assert.ThrowsAsync<DbUpdateException>(() => unitOfWork.SaveChangesAsync(CancellationToken.None));
    }
}
