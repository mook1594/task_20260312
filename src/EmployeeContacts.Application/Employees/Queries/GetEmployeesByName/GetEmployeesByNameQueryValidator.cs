using FluentValidation;

namespace EmployeeContacts.Application.Employees.Queries.GetEmployeesByName;

public sealed class GetEmployeesByNameQueryValidator : AbstractValidator<GetEmployeesByNameQuery>
{
    public GetEmployeesByNameQueryValidator()
    {
        RuleFor(query => query.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name?.Trim()))
            .WithMessage("'Name' must not be empty.");
    }
}
