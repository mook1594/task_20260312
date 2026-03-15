namespace EmployeeContacts.Infrastructure.Persistence.Entities;

public sealed class EmployeeEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public DateOnly Joined { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
