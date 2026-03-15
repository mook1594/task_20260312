using System.Globalization;
using EmployeeContacts.Application.Abstractions.Persistence;
using EmployeeContacts.Domain.Common;
using EmployeeContacts.Domain.Employees;
using EmployeeContacts.Domain.Employees.ValueObjects;
using MediatR;

namespace EmployeeContacts.Application.Employees.Commands.BulkCreateEmployees;

public sealed class BulkCreateEmployeesCommandHandler
    : IRequestHandler<BulkCreateEmployeesCommand, BulkCreateEmployeesResult>
{
    private readonly IEmployeeRepository employeeRepository;
    private readonly IUnitOfWork unitOfWork;

    public BulkCreateEmployeesCommandHandler(IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
    {
        this.employeeRepository = employeeRepository;
        this.unitOfWork = unitOfWork;
    }

    public async Task<BulkCreateEmployeesResult> Handle(
        BulkCreateEmployeesCommand request,
        CancellationToken cancellationToken)
    {
        BulkCreateEmployeeErrorMapper.BulkCreateEmployeeRowError?[] rowErrors =
            new BulkCreateEmployeeErrorMapper.BulkCreateEmployeeRowError?[request.Records.Count];
        List<NormalizedBulkEmployee> normalizedEmployees = [];

        for (int index = 0; index < request.Records.Count; index++)
        {
            BulkEmployeeRecord record = request.Records[index];

            if (TryNormalize(
                    index,
                    record,
                    out NormalizedBulkEmployee? normalizedEmployee,
                    out BulkCreateEmployeeErrorMapper.BulkCreateEmployeeRowError? error))
            {
                normalizedEmployees.Add(normalizedEmployee!);
                continue;
            }

            rowErrors[index] = error;
        }

        List<NormalizedBulkEmployee> requestUniqueEmployees = MarkRequestDuplicates(normalizedEmployees, rowErrors);
        List<Employee> employeesToCreate = await BuildEmployeesToCreateAsync(
            requestUniqueEmployees,
            rowErrors,
            cancellationToken).ConfigureAwait(false);

        if (employeesToCreate.Count > 0)
        {
            await employeeRepository.AddRangeAsync(employeesToCreate, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        IReadOnlyList<BulkCreateEmployeesError> errors = rowErrors
            .Where(error => error is not null)
            .Select(error => BulkCreateEmployeeErrorMapper.ToContract(error!))
            .ToArray();

        return new BulkCreateEmployeesResult(
            request.Records.Count,
            employeesToCreate.Count,
            errors.Count,
            errors);
    }

    private static bool TryNormalize(
        int index,
        BulkEmployeeRecord record,
        out NormalizedBulkEmployee? normalizedEmployee,
        out BulkCreateEmployeeErrorMapper.BulkCreateEmployeeRowError? error)
    {
        normalizedEmployee = null;
        error = null;

        EmployeeName name;
        try
        {
            name = EmployeeName.Create(record.Name);
        }
        catch (DomainException exception)
        {
            error = BulkCreateEmployeeErrorMapper.FromDomainException(record.Row, exception);
            return false;
        }

        EmployeeEmail email;
        try
        {
            email = EmployeeEmail.Create(record.Email);
        }
        catch (DomainException exception)
        {
            error = BulkCreateEmployeeErrorMapper.FromDomainException(record.Row, exception);
            return false;
        }

        EmployeePhoneNumber phoneNumber;
        try
        {
            phoneNumber = EmployeePhoneNumber.Create(record.Tel);
        }
        catch (DomainException exception)
        {
            error = BulkCreateEmployeeErrorMapper.FromDomainException(record.Row, exception);
            return false;
        }

        DateOnly joined;
        if (!DateOnly.TryParseExact(
                record.Joined,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out joined))
        {
            error = BulkCreateEmployeeErrorMapper.InvalidJoined(record.Row);
            return false;
        }

        normalizedEmployee = new NormalizedBulkEmployee(index, record.Row, name, email, phoneNumber, joined);
        return true;
    }

    private static List<NormalizedBulkEmployee> MarkRequestDuplicates(
        IEnumerable<NormalizedBulkEmployee> normalizedEmployees,
        BulkCreateEmployeeErrorMapper.BulkCreateEmployeeRowError?[] rowErrors)
    {
        HashSet<string> seenEmails = new(StringComparer.Ordinal);
        HashSet<string> seenPhoneNumbers = new(StringComparer.Ordinal);
        List<NormalizedBulkEmployee> requestUniqueEmployees = [];

        foreach (NormalizedBulkEmployee normalizedEmployee in normalizedEmployees.OrderBy(employee => employee.Index))
        {
            if (!seenEmails.Add(normalizedEmployee.NormalizedEmail))
            {
                rowErrors[normalizedEmployee.Index] = BulkCreateEmployeeErrorMapper.DuplicateEmail(normalizedEmployee.Row);
                continue;
            }

            if (!seenPhoneNumbers.Add(normalizedEmployee.NormalizedPhoneNumber))
            {
                rowErrors[normalizedEmployee.Index] = BulkCreateEmployeeErrorMapper.DuplicateTel(normalizedEmployee.Row);
                continue;
            }

            requestUniqueEmployees.Add(normalizedEmployee);
        }

        return requestUniqueEmployees;
    }

    private async Task<List<Employee>> BuildEmployeesToCreateAsync(
        IReadOnlyList<NormalizedBulkEmployee> requestUniqueEmployees,
        BulkCreateEmployeeErrorMapper.BulkCreateEmployeeRowError?[] rowErrors,
        CancellationToken cancellationToken)
    {
        if (requestUniqueEmployees.Count == 0)
        {
            return [];
        }

        string[] emails = requestUniqueEmployees.Select(employee => employee.NormalizedEmail).Distinct(StringComparer.Ordinal).ToArray();
        string[] phoneNumbers = requestUniqueEmployees.Select(employee => employee.NormalizedPhoneNumber).Distinct(StringComparer.Ordinal).ToArray();

        IReadOnlySet<string> existingEmails = await employeeRepository
            .GetExistingEmailsAsync(emails, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlySet<string> existingPhoneNumbers = await employeeRepository.GetExistingPhoneNumbersAsync(phoneNumbers, cancellationToken).ConfigureAwait(false);

        List<Employee> employeesToCreate = [];

        foreach (NormalizedBulkEmployee normalizedEmployee in requestUniqueEmployees.OrderBy(employee => employee.Index))
        {
            if (existingEmails.Contains(normalizedEmployee.NormalizedEmail))
            {
                rowErrors[normalizedEmployee.Index] = BulkCreateEmployeeErrorMapper.DuplicateEmail(normalizedEmployee.Row);
                continue;
            }

            if (existingPhoneNumbers.Contains(normalizedEmployee.NormalizedPhoneNumber))
            {
                rowErrors[normalizedEmployee.Index] = BulkCreateEmployeeErrorMapper.DuplicateTel(normalizedEmployee.Row);
                continue;
            }

            if (TryCreateEmployee(
                    normalizedEmployee,
                    out Employee? employee,
                    out BulkCreateEmployeeErrorMapper.BulkCreateEmployeeRowError? error))
            {
                employeesToCreate.Add(employee!);
                continue;
            }

            rowErrors[normalizedEmployee.Index] = error;
        }

        return employeesToCreate;
    }

    private static bool TryCreateEmployee(
        NormalizedBulkEmployee normalizedEmployee,
        out Employee? employee,
        out BulkCreateEmployeeErrorMapper.BulkCreateEmployeeRowError? error)
    {
        employee = null;
        error = null;

        try
        {
            employee = Employee.Create(
                Guid.CreateVersion7(),
                normalizedEmployee.Name,
                normalizedEmployee.Email,
                normalizedEmployee.PhoneNumber,
                normalizedEmployee.Joined);
            return true;
        }
        catch (DomainException exception)
        {
            error = BulkCreateEmployeeErrorMapper.FromDomainException(normalizedEmployee.Row, exception);
            return false;
        }
    }

    private sealed record NormalizedBulkEmployee(
        int Index,
        int Row,
        EmployeeName Name,
        EmployeeEmail Email,
        EmployeePhoneNumber PhoneNumber,
        DateOnly Joined)
    {
        public string NormalizedEmail => Email.Value;

        public string NormalizedPhoneNumber => PhoneNumber.Value;
    }
}
