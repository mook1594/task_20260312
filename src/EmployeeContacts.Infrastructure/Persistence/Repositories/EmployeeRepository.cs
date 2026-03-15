using System.Linq.Expressions;
using EmployeeContacts.Application.Abstractions.Persistence;
using EmployeeContacts.Application.Common.Models;
using EmployeeContacts.Application.Employees.Dtos;
using EmployeeContacts.Domain.Employees;
using EmployeeContacts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeContacts.Infrastructure.Persistence.Repositories;

public sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext dbContext;

    public EmployeeRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<PagedResult<EmployeeDto>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<EmployeeEntity> query = dbContext.Employees
            .AsNoTracking()
            .OrderBy(employee => employee.Name)
            .ThenBy(employee => employee.Id);

        int totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        List<EmployeeDto> items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToDtoExpression)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        int totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<EmployeeDto>(items, page, pageSize, totalCount, totalPages);
    }

    public async Task<IReadOnlyList<EmployeeDto>> GetByNameAsync(string exactName, CancellationToken cancellationToken)
        => await dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.Name == exactName)
            .OrderBy(employee => employee.Name)
            .ThenBy(employee => employee.Id)
            .Select(ToDtoExpression)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlySet<string>> GetExistingEmailsAsync(
        IReadOnlyCollection<string> emails,
        CancellationToken cancellationToken)
    {
        if (emails.Count == 0)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        List<string> results = await dbContext.Employees
            .AsNoTracking()
            .Where(employee => emails.Contains(employee.Email))
            .Select(employee => employee.Email)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return results.ToHashSet(StringComparer.Ordinal);
    }

    public async Task<IReadOnlySet<string>> GetExistingPhoneNumbersAsync(
        IReadOnlyCollection<string> phoneNumbers,
        CancellationToken cancellationToken)
    {
        if (phoneNumbers.Count == 0)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        List<string> results = await dbContext.Employees
            .AsNoTracking()
            .Where(employee => phoneNumbers.Contains(employee.PhoneNumber))
            .Select(employee => employee.PhoneNumber)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return results.ToHashSet(StringComparer.Ordinal);
    }

    public Task AddRangeAsync(IReadOnlyCollection<Employee> employees, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        List<EmployeeEntity> entities = employees
            .Select(employee => new EmployeeEntity
            {
                Id = employee.Id,
                Name = employee.Name.Value,
                Email = employee.Email.Value,
                PhoneNumber = employee.PhoneNumber.Value,
                Joined = employee.Joined,
                CreatedAt = employee.CreatedAt,
                UpdatedAt = employee.UpdatedAt
            })
            .ToList();

        return dbContext.Employees.AddRangeAsync(entities, cancellationToken);
    }

    private static readonly Expression<Func<EmployeeEntity, EmployeeDto>> ToDtoExpression =
        employee => new EmployeeDto(
            employee.Id,
            employee.Name,
            employee.Email,
            employee.PhoneNumber,
            employee.Joined);
}
