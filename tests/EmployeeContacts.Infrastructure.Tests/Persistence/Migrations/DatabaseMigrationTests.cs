namespace EmployeeContacts.Infrastructure.Tests.Persistence.Migrations;

public class DatabaseMigrationTests
{
    [Fact(DisplayName = "초기 마이그레이션이 빈 데이터베이스에 적용 가능하다.")]
    public async Task DatabaseMigration_ShouldApplyInitialCreateMigration()
    {
        await using InfrastructureTestHost host = InfrastructureTestHost.Create();

        await host.MigrateAsync();

        await using DbContext dbContext = host.GetDbContext();
        string[] tables = await QuerySingleColumnAsync(dbContext, "SELECT name FROM sqlite_master WHERE type = 'table';");

        Assert.Contains("Employees", tables);
    }

    [Fact(DisplayName = "Email, PhoneNumber 유니크 인덱스가 생성된다.")]
    public async Task DatabaseSchema_ShouldCreateUniqueIndexes_ForEmailAndPhoneNumber()
    {
        await using InfrastructureTestHost host = InfrastructureTestHost.Create();
        await host.MigrateAsync();

        await using DbContext dbContext = host.GetDbContext();
        string[] indexes = await QuerySingleColumnAsync(dbContext, "SELECT name FROM pragma_index_list('Employees');");

        Assert.Contains(indexes, index => index.Contains("Email", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(indexes, index => index.Contains("PhoneNumber", StringComparison.OrdinalIgnoreCase));
    }

    [Fact(DisplayName = "Name 조회 인덱스가 생성된다.")]
    public async Task DatabaseSchema_ShouldCreateIndex_ForName()
    {
        await using InfrastructureTestHost host = InfrastructureTestHost.Create();
        await host.MigrateAsync();

        await using DbContext dbContext = host.GetDbContext();
        string[] indexes = await QuerySingleColumnAsync(dbContext, "SELECT name FROM pragma_index_list('Employees');");

        Assert.Contains(indexes, index => index.Contains("Name", StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<string[]> QuerySingleColumnAsync(DbContext dbContext, string sql)
    {
        DbConnection connection = dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using DbCommand command = connection.CreateCommand();
        command.CommandText = sql;

        List<string> results = [];
        await using DbDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(reader.GetValue(0).ToString()!);
        }

        return [.. results];
    }
}
