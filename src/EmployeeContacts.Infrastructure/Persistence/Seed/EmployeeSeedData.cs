using EmployeeContacts.Infrastructure.Persistence.Entities;

namespace EmployeeContacts.Infrastructure.Persistence.Seed;

internal static class EmployeeSeedData
{
    private static readonly DateTimeOffset SeedTimestamp = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static IReadOnlyList<EmployeeEntity> DefaultEmployees { get; } =
    [
        new EmployeeEntity
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "기본 관리자",
            Email = "admin@employeecontacts.local",
            PhoneNumber = "01090000001",
            Joined = new DateOnly(2020, 1, 1),
            CreatedAt = SeedTimestamp,
            UpdatedAt = SeedTimestamp
        },
        new EmployeeEntity
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "기본 담당자",
            Email = "operator@employeecontacts.local",
            PhoneNumber = "01090000002",
            Joined = new DateOnly(2020, 1, 2),
            CreatedAt = SeedTimestamp,
            UpdatedAt = SeedTimestamp
        },
        new EmployeeEntity
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "김철수",
            Email = "kim.chulsu@company.com",
            PhoneNumber = "01012345678",
            Joined = new DateOnly(2021, 3, 15),
            CreatedAt = SeedTimestamp,
            UpdatedAt = SeedTimestamp
        },
        new EmployeeEntity
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Name = "이영미",
            Email = "lee.youngmi@company.com",
            PhoneNumber = "01087654321",
            Joined = new DateOnly(2021, 6, 20),
            CreatedAt = SeedTimestamp,
            UpdatedAt = SeedTimestamp
        },
        new EmployeeEntity
        {
            Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            Name = "박민준",
            Email = "park.minjun@company.com",
            PhoneNumber = "01056781234",
            Joined = new DateOnly(2022, 1, 10),
            CreatedAt = SeedTimestamp,
            UpdatedAt = SeedTimestamp
        },
        new EmployeeEntity
        {
            Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
            Name = "정수영",
            Email = "jung.suyoung@company.com",
            PhoneNumber = "01034567890",
            Joined = new DateOnly(2022, 8, 5),
            CreatedAt = SeedTimestamp,
            UpdatedAt = SeedTimestamp
        },
        new EmployeeEntity
        {
            Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
            Name = "최준호",
            Email = "choi.junho@company.com",
            PhoneNumber = "01098765432",
            Joined = new DateOnly(2023, 2, 14),
            CreatedAt = SeedTimestamp,
            UpdatedAt = SeedTimestamp
        },
        new EmployeeEntity
        {
            Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
            Name = "강현지",
            Email = "kang.hyunji@company.com",
            PhoneNumber = "01076543210",
            Joined = new DateOnly(2023, 5, 1),
            CreatedAt = SeedTimestamp,
            UpdatedAt = SeedTimestamp
        }
    ];
}
