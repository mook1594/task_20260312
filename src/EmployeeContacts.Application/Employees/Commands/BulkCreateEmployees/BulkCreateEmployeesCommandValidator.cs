using FluentValidation;

namespace EmployeeContacts.Application.Employees.Commands.BulkCreateEmployees;

public sealed class BulkCreateEmployeesCommandValidator : AbstractValidator<BulkCreateEmployeesCommand>
{
    public BulkCreateEmployeesCommandValidator()
    {
        RuleFor(command => command.Records).NotNull();
        RuleFor(command => command.Records).Must(records => records is { Count: > 0 });

        RuleForEach(command => command.Records)
            .ChildRules(record =>
            {
                record.RuleFor(value => value.Row).GreaterThanOrEqualTo(1);
            });
    }
}
