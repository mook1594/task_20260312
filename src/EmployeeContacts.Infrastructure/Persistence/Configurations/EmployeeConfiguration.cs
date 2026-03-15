using System.Globalization;
using EmployeeContacts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EmployeeContacts.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<EmployeeEntity>
{
    public void Configure(EntityTypeBuilder<EmployeeEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        ValueConverter<DateOnly, string> dateOnlyConverter = new(
            value => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            value => DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture));

        ValueConverter<DateTimeOffset, string> dateTimeOffsetConverter = new(
            value => value.ToString("O", CultureInfo.InvariantCulture),
            value => DateTimeOffset.ParseExact(value, "O", CultureInfo.InvariantCulture));

        builder.ToTable("Employees");
        builder.HasKey(employee => employee.Id);

        builder.Property(employee => employee.Name)
            .IsRequired();

        builder.Property(employee => employee.Email)
            .IsRequired();

        builder.Property(employee => employee.PhoneNumber)
            .IsRequired()
            .HasMaxLength(15);

        builder.Property(employee => employee.Joined)
            .IsRequired()
            .HasConversion(dateOnlyConverter)
            .HasColumnType("TEXT");

        builder.Property(employee => employee.CreatedAt)
            .IsRequired()
            .HasConversion(dateTimeOffsetConverter)
            .HasColumnType("TEXT");

        builder.Property(employee => employee.UpdatedAt)
            .IsRequired()
            .HasConversion(dateTimeOffsetConverter)
            .HasColumnType("TEXT");

        builder.HasIndex(employee => employee.Email)
            .IsUnique();

        builder.HasIndex(employee => employee.PhoneNumber)
            .IsUnique();

        builder.HasIndex(employee => employee.Name);
    }
}
