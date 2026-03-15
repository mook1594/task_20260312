namespace EmployeeContacts.Infrastructure.Tests.DependencyInjection;

public class DependencyInjectionTests
{
    [Fact(DisplayName = "AddInfrastructure는 repository, unit of work, parser, detector, DbContext를 등록한다.")]
    public void AddInfrastructure_ShouldRegisterInfrastructureServices()
    {
        ServiceCollection services = [];
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:EmployeeContacts"] = "Data Source=:memory:"
            })
            .Build();

        InfrastructureTestHost.ApplyInfrastructureRegistration(services, configuration);

        Type dbContextType = InfrastructureTestHost.GetRequiredType("EmployeeContacts.Infrastructure.Persistence.AppDbContext");
        Type csvParserType = InfrastructureTestHost.GetRequiredType("EmployeeContacts.Infrastructure.Parsing.Csv.CsvEmployeeImportParser");
        Type jsonParserType = InfrastructureTestHost.GetRequiredType("EmployeeContacts.Infrastructure.Parsing.Json.JsonEmployeeImportParser");
        Type detectorType = InfrastructureTestHost.GetRequiredType("EmployeeContacts.Infrastructure.Parsing.Text.PlainTextEmployeeImportDetector");

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == dbContextType
            && descriptor.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(IEmployeeRepository)
            && descriptor.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(IUnitOfWork)
            && descriptor.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(IEmployeeImportParser)
            && descriptor.ImplementationType == csvParserType
            && descriptor.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(IEmployeeImportParser)
            && descriptor.ImplementationType == jsonParserType
            && descriptor.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(IPlainTextEmployeeImportDetector)
            && descriptor.ImplementationType == detectorType
            && descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact(DisplayName = "연결 문자열이 없으면 AddInfrastructure는 즉시 실패한다.")]
    public void AddInfrastructure_ShouldThrow_WhenConnectionStringIsMissing()
    {
        ServiceCollection services = [];
        IConfiguration configuration = new ConfigurationBuilder().Build();

        Assert.ThrowsAny<Exception>(() => InfrastructureTestHost.ApplyInfrastructureRegistration(services, configuration));
    }
}
