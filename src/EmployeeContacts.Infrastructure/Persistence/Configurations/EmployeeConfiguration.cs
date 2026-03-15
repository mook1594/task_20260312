using System.Globalization;
using EmployeeContacts.Infrastructure.Persistence.Entities;
using EmployeeContacts.Infrastructure.Persistence.Seed;
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

        builder.HasData(
            new EmployeeEntity
            {
                Id = new Guid("11111111-1111-1111-1111-111111111111"),
                Name = "김철수",
                Email = "charles@clovf.com",
                PhoneNumber = "01075312468",
                Joined = new DateOnly(2018, 03, 07),
                CreatedAt = new DateTimeOffset(2024, 01, 01, 0, 0, 0, TimeSpan.Zero),
                UpdatedAt = new DateTimeOffset(2024, 01, 01, 0, 0, 0, TimeSpan.Zero),
            },
            new EmployeeEntity
            {
                Id = new Guid("22222222-2222-2222-2222-222222222222"),
                Name = "박영희",
                Email = "matilda@clovf.com",
                PhoneNumber = "01087654321",
                Joined = new DateOnly(2021, 04, 28),
                CreatedAt = new DateTimeOffset(2024, 01, 01, 0, 0, 0, TimeSpan.Zero),
                UpdatedAt = new DateTimeOffset(2024, 01, 01, 0, 0, 0, TimeSpan.Zero),
            },
            new EmployeeEntity
            {
                Id = new Guid("33333333-3333-3333-3333-333333333333"),
                Name = "홍길동",
                Email = "kildong.hong@clovf.com",
                PhoneNumber = "01012345678",
                Joined = new DateOnly(2015, 08, 15),
                CreatedAt = new DateTimeOffset(2024, 01, 01, 0, 0, 0, TimeSpan.Zero),
                UpdatedAt = new DateTimeOffset(2024, 01, 01, 0, 0, 0, TimeSpan.Zero),
            },
            new EmployeeEntity
            {
                Id = new Guid("44444444-4444-4444-4444-444444444444"),
                Name = "김클로",
                Email = "clo@clovf.com",
                PhoneNumber = "0101111-2424",
                Joined = new DateOnly(2012, 01, 05),
                CreatedAt = new DateTimeOffset(2024, 01, 01, 0, 0, 0, TimeSpan.Zero),
                UpdatedAt = new DateTimeOffset(2024, 01, 01, 0, 0, 0, TimeSpan.Zero),
            },
            new EmployeeEntity
            {
                Id = new Guid("55555555-5555-5555-5555-555555555555"),
                Name = "박마블",
                Email = "md@clovf.com",
                PhoneNumber = "0103535-7979",
                Joined = new DateOnly(2013, 07, 01),
                CreatedAt = new DateTimeOffset(2024, 01, 01, 0, 0, 0, TimeSpan.Zero),
                UpdatedAt = new DateTimeOffset(2024, 01, 01, 0, 0, 0, TimeSpan.Zero),
            },
            new EmployeeEntity
            {
                Id = new Guid("66666666-6666-6666-6666-666666666666"),
                Name = "홍커넥",
                Email = "connect@clovf.com",
                PhoneNumber = "0108531-7942",
                Joined = new DateOnly(2019, 12, 05),
                CreatedAt = new DateTimeOffset(2024, 01, 01, 0, 0, 0, TimeSpan.Zero),
                UpdatedAt = new DateTimeOffset(2024, 01, 01, 0, 0, 0, TimeSpan.Zero),
            });
    }
}
