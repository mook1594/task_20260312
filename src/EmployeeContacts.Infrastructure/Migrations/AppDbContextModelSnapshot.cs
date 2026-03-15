using System.Globalization;
using EmployeeContacts.Infrastructure.Persistence;
using EmployeeContacts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EmployeeContacts.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
public partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        ValueConverter<DateOnly, string> dateOnlyConverter = new(
            value => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            value => DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture));

        ValueConverter<DateTimeOffset, string> dateTimeOffsetConverter = new(
            value => value.ToString("O", CultureInfo.InvariantCulture),
            value => DateTimeOffset.ParseExact(value, "O", CultureInfo.InvariantCulture));

        modelBuilder.Entity<EmployeeEntity>(entity =>
        {
            entity.ToTable("Employees");

            entity.HasKey(employee => employee.Id);

            entity.Property(employee => employee.Name)
                .IsRequired();

            entity.Property(employee => employee.Email)
                .IsRequired();

            entity.Property(employee => employee.PhoneNumber)
                .IsRequired()
                .HasMaxLength(15);

            entity.Property(employee => employee.Joined)
                .HasConversion(dateOnlyConverter)
                .HasColumnType("TEXT");

            entity.Property(employee => employee.CreatedAt)
                .HasConversion(dateTimeOffsetConverter)
                .HasColumnType("TEXT");

            entity.Property(employee => employee.UpdatedAt)
                .HasConversion(dateTimeOffsetConverter)
                .HasColumnType("TEXT");

            entity.HasIndex(employee => employee.Email)
                .IsUnique();

            entity.HasIndex(employee => employee.Name);

            entity.HasIndex(employee => employee.PhoneNumber)
                .IsUnique();
        });
    }
}
