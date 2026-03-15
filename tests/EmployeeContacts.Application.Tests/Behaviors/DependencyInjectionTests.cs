using System.Diagnostics;
using EmployeeContacts.Application.Behaviors;
using EmployeeContacts.Application.Common.Models;
using EmployeeContacts.Application.DependencyInjection;
using EmployeeContacts.Application.Employees.Dtos;
using EmployeeContacts.Application.Employees.Queries.GetEmployees;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EmployeeContacts.Application.Tests.Behaviors;

public class DependencyInjectionTests
{
    [Fact(DisplayName = "AddApplication은 MediatR, Validators, Behaviors를 등록한다.")]
    public void AddApplication_ShouldRegisterApplicationServices()
    {
        ServiceCollection services = new();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        services.AddApplication();
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        IMediator? mediator = serviceProvider.GetService<IMediator>();
        IValidator<GetEmployeesQuery>? validator = serviceProvider.GetService<IValidator<GetEmployeesQuery>>();
        ActivitySource? activitySource = serviceProvider.GetService<ActivitySource>();
        IPipelineBehavior<GetEmployeesQuery, PagedResult<EmployeeDto>>[] behaviors = serviceProvider
            .GetServices<IPipelineBehavior<GetEmployeesQuery, PagedResult<EmployeeDto>>>()
            .ToArray();

        Assert.NotNull(mediator);
        Assert.NotNull(validator);
        Assert.NotNull(activitySource);
        Assert.Equal(3, behaviors.Length);
        Assert.IsType<LoggingBehavior<GetEmployeesQuery, PagedResult<EmployeeDto>>>(behaviors[0]);
        Assert.IsType<ValidationBehavior<GetEmployeesQuery, PagedResult<EmployeeDto>>>(behaviors[1]);
        Assert.IsType<TracingBehavior<GetEmployeesQuery, PagedResult<EmployeeDto>>>(behaviors[2]);
    }
}
