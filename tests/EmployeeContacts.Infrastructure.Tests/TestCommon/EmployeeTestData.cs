namespace EmployeeContacts.Infrastructure.Tests.TestCommon;

internal static class EmployeeTestData
{
    public static Employee CreateEmployee(
        Guid id,
        string name,
        string email,
        string tel,
        string joined)
        => Employee.Create(
            id,
            EmployeeName.Create(name),
            EmployeeEmail.Create(email),
            EmployeePhoneNumber.Create(tel),
            DateOnly.ParseExact(joined, "yyyy-MM-dd", CultureInfo.InvariantCulture));
}
