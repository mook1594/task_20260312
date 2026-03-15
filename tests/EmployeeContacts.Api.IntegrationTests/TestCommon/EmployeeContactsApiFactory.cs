using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using EmployeeContacts.Infrastructure.DependencyInjection;

namespace EmployeeContacts.Api.IntegrationTests.TestCommon;

internal sealed class EmployeeContactsApiFactory : WebApplicationFactory<Program>
{
    private readonly string databasePath;
    private readonly Action<IServiceCollection>? configureServices;
    private bool databaseInitialized;

    public EmployeeContactsApiFactory(Action<IServiceCollection>? configureServices = null)
    {
        this.configureServices = configureServices;
        databasePath = Path.Combine(
            Path.GetTempPath(),
            "employee-contacts-api-tests",
            $"{Guid.NewGuid():N}.db");

        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:EmployeeContacts"] = $"Data Source={databasePath}"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(
                    $"Data Source={databasePath}",
                    sqlite => sqlite.MigrationsAssembly(typeof(DependencyInjection).Assembly.GetName().Name)));

            configureServices?.Invoke(services);
        });
    }

    public HttpClient CreateApiClient()
    {
        EnsureDatabaseInitialized();

        return CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    public async Task SeedEmployeesAsync(params EmployeeEntity[] employees)
    {
        await EnsureDatabaseInitializedAsync().ConfigureAwait(false);

        await using AppDbContext dbContext = CreateDbContext();
        dbContext.Employees.AddRange(employees);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public new void Dispose()
    {
        base.Dispose();

        if (File.Exists(databasePath))
        {
            File.Delete(databasePath);
        }
    }

    private void EnsureDatabaseInitialized()
        => EnsureDatabaseInitializedAsync().GetAwaiter().GetResult();

    private async Task EnsureDatabaseInitializedAsync()
    {
        if (databaseInitialized)
        {
            return;
        }

        await using AppDbContext dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync().ConfigureAwait(false);
        await dbContext.Employees.ExecuteDeleteAsync().ConfigureAwait(false);
        databaseInitialized = true;
    }

    private AppDbContext CreateDbContext()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(
                $"Data Source={databasePath}",
                sqlite => sqlite.MigrationsAssembly(typeof(DependencyInjection).Assembly.GetName().Name))
            .Options;

        return new AppDbContext(options);
    }
}
