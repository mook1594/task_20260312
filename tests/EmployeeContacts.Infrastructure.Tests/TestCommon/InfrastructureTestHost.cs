namespace EmployeeContacts.Infrastructure.Tests.TestCommon;

internal sealed class InfrastructureTestHost : IAsyncDisposable
{
    private readonly string databasePath;

    private InfrastructureTestHost(
        string databasePath,
        ServiceCollection services,
        ServiceProvider serviceProvider,
        IServiceScope scope)
    {
        this.databasePath = databasePath;
        Services = services;
        ServiceProvider = serviceProvider;
        Scope = scope;
    }

    public ServiceCollection Services { get; }

    public ServiceProvider ServiceProvider { get; }

    public IServiceScope Scope { get; }

    public static InfrastructureTestHost Create(bool includeConnectionString = true)
    {
        string databasePath = Path.Combine(
            Path.GetTempPath(),
            "employee-contacts-infrastructure-tests",
            $"{Guid.NewGuid():N}.db");

        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

        ServiceCollection services = [];
        IConfiguration configuration = CreateConfiguration(databasePath, includeConnectionString);

        ApplyInfrastructureRegistration(services, configuration);

        ServiceProvider serviceProvider = services.BuildServiceProvider(validateScopes: true);
        IServiceScope scope = serviceProvider.CreateScope();

        return new InfrastructureTestHost(databasePath, services, serviceProvider, scope);
    }

    public IEmployeeRepository GetEmployeeRepository()
        => Scope.ServiceProvider.GetRequiredService<IEmployeeRepository>();

    public IUnitOfWork GetUnitOfWork()
        => Scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

    public IPlainTextEmployeeImportDetector GetPlainTextDetector()
        => Scope.ServiceProvider.GetRequiredService<IPlainTextEmployeeImportDetector>();

    public IReadOnlyList<IEmployeeImportParser> GetParsers()
        => Scope.ServiceProvider.GetServices<IEmployeeImportParser>().ToArray();

    public DbContext GetDbContext()
    {
        Type dbContextType = GetRequiredType("EmployeeContacts.Infrastructure.Persistence.AppDbContext");
        object? service = Scope.ServiceProvider.GetService(dbContextType);

        return service as DbContext
            ?? throw new XunitException(
                $"'{dbContextType.FullName}' must be registered as a DbContext service.");
    }

    public async Task MigrateAsync()
    {
        DbContext dbContext = GetDbContext();
        await dbContext.Database.MigrateAsync().ConfigureAwait(false);
    }

    public static void ApplyInfrastructureRegistration(IServiceCollection services, IConfiguration configuration)
    {
        Type dependencyInjectionType = GetRequiredType("EmployeeContacts.Infrastructure.DependencyInjection.DependencyInjection");
        MethodInfo? addInfrastructureMethod = dependencyInjectionType.GetMethod(
            "AddInfrastructure",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(IServiceCollection), typeof(IConfiguration)]);

        if (addInfrastructureMethod is null)
        {
            throw new XunitException(
                "EmployeeContacts.Infrastructure.DependencyInjection.DependencyInjection.AddInfrastructure(IServiceCollection, IConfiguration) must exist.");
        }

        _ = addInfrastructureMethod.Invoke(null, [services, configuration])
            ?? throw new XunitException("AddInfrastructure must return IServiceCollection.");
    }

    public static IEmployeeImportParser CreateCsvParser()
        => CreateParser("EmployeeContacts.Infrastructure.Parsing.Csv.CsvEmployeeImportParser");

    public static IEmployeeImportParser CreateJsonParser()
        => CreateParser("EmployeeContacts.Infrastructure.Parsing.Json.JsonEmployeeImportParser");

    public static IPlainTextEmployeeImportDetector CreatePlainTextDetector()
    {
        object csvParser = CreateCsvParser();
        object jsonParser = CreateJsonParser();
        Type detectorType = GetRequiredType("EmployeeContacts.Infrastructure.Parsing.Text.PlainTextEmployeeImportDetector");
        object detector = CreateInstance(detectorType, [csvParser, jsonParser]);

        return detector as IPlainTextEmployeeImportDetector
            ?? throw new XunitException(
                $"'{detectorType.FullName}' must implement {nameof(IPlainTextEmployeeImportDetector)}.");
    }

    public static Type GetRequiredType(string fullName)
        => InfrastructureAssembly.GetType(fullName)
            ?? throw new XunitException(
                $"'{fullName}' type must be added to EmployeeContacts.Infrastructure.");

    public async ValueTask DisposeAsync()
    {
        if (Scope is IAsyncDisposable asyncScope)
        {
            await asyncScope.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            Scope.Dispose();
        }

        await ServiceProvider.DisposeAsync().ConfigureAwait(false);

        if (File.Exists(databasePath))
        {
            File.Delete(databasePath);
        }
    }

    private static IConfiguration CreateConfiguration(string databasePath, bool includeConnectionString)
    {
        Dictionary<string, string?> values = [];

        if (includeConnectionString)
        {
            values["ConnectionStrings:EmployeeContacts"] = $"Data Source={databasePath}";
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static IEmployeeImportParser CreateParser(string fullName)
    {
        Type parserType = GetRequiredType(fullName);
        object parser = CreateInstance(parserType, []);

        return parser as IEmployeeImportParser
            ?? throw new XunitException($"'{parserType.FullName}' must implement {nameof(IEmployeeImportParser)}.");
    }

    private static object CreateInstance(Type type, IReadOnlyList<object> availableArguments)
    {
        ConstructorInfo[] constructors = type.GetConstructors()
            .OrderBy(constructor => constructor.GetParameters().Length)
            .ToArray();

        if (constructors.Length == 0)
        {
            throw new XunitException($"'{type.FullName}' must expose a public constructor.");
        }

        foreach (ConstructorInfo constructor in constructors)
        {
            object?[]? arguments = TryMatchArguments(constructor.GetParameters(), availableArguments);
            if (arguments is null)
            {
                continue;
            }

            return constructor.Invoke(arguments);
        }

        throw new XunitException($"Unable to create '{type.FullName}' with the supported test arguments.");
    }

    private static object?[]? TryMatchArguments(
        IReadOnlyList<ParameterInfo> parameters,
        IReadOnlyList<object> availableArguments)
    {
        object?[] arguments = new object?[parameters.Count];
        List<object> remainingArguments = [.. availableArguments];

        for (int index = 0; index < parameters.Count; index++)
        {
            ParameterInfo parameter = parameters[index];
            object? match = MatchNamedDependency(parameter, remainingArguments)
                ?? remainingArguments.FirstOrDefault(argument => parameter.ParameterType.IsInstanceOfType(argument));

            if (match is not null)
            {
                arguments[index] = match;
                remainingArguments.Remove(match);
                continue;
            }

            if (parameter.HasDefaultValue)
            {
                arguments[index] = parameter.DefaultValue;
                continue;
            }

            return null;
        }

        return arguments;
    }

    private static object? MatchNamedDependency(ParameterInfo parameter, IReadOnlyList<object> availableArguments)
    {
        string parameterName = parameter.Name ?? string.Empty;

        if (parameterName.Contains("json", StringComparison.OrdinalIgnoreCase))
        {
            return availableArguments.FirstOrDefault(argument =>
                argument.GetType().Name.Contains("Json", StringComparison.OrdinalIgnoreCase)
                && parameter.ParameterType.IsInstanceOfType(argument));
        }

        if (parameterName.Contains("csv", StringComparison.OrdinalIgnoreCase))
        {
            return availableArguments.FirstOrDefault(argument =>
                argument.GetType().Name.Contains("Csv", StringComparison.OrdinalIgnoreCase)
                && parameter.ParameterType.IsInstanceOfType(argument));
        }

        return null;
    }

    private static Assembly InfrastructureAssembly => infrastructureAssembly ??= LoadInfrastructureAssembly();

    private static Assembly? infrastructureAssembly;

    private static Assembly LoadInfrastructureAssembly()
    {
        try
        {
            return Assembly.Load("EmployeeContacts.Infrastructure");
        }
        catch (Exception exception)
        {
            throw new XunitException(
                $"Unable to load EmployeeContacts.Infrastructure assembly. {exception.Message}");
        }
    }
}
