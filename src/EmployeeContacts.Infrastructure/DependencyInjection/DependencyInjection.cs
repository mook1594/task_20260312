using EmployeeContacts.Application.Abstractions.Parsing;
using EmployeeContacts.Application.Abstractions.Persistence;
using EmployeeContacts.Infrastructure.Parsing.Csv;
using EmployeeContacts.Infrastructure.Parsing.Json;
using EmployeeContacts.Infrastructure.Parsing.Text;
using EmployeeContacts.Infrastructure.Persistence;
using EmployeeContacts.Infrastructure.Persistence.Repositories;
using EmployeeContacts.Infrastructure.Persistence.UnitOfWork;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeContacts.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        string? connectionString = configuration.GetConnectionString("EmployeeContacts");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:EmployeeContacts is required.");
        }

        SqliteConnectionStringBuilder connectionStringBuilder = new(connectionString)
        {
            Pooling = false
        };

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(
                connectionStringBuilder.ToString(),
                sqlite => sqlite.MigrationsAssembly(typeof(DependencyInjection).Assembly.GetName().Name)));

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddSingleton<CsvEmployeeImportParser>();
        services.AddSingleton<JsonEmployeeImportParser>();
        services.AddSingleton<IEmployeeImportParser, CsvEmployeeImportParser>();
        services.AddSingleton<IEmployeeImportParser, JsonEmployeeImportParser>();
        services.AddSingleton<IPlainTextEmployeeImportDetector, PlainTextEmployeeImportDetector>();

        return services;
    }
}
