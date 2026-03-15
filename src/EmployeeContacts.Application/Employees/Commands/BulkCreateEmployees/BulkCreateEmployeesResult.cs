namespace EmployeeContacts.Application.Employees.Commands.BulkCreateEmployees;

/// <summary>
/// 직원 일괄 등록 처리 결과다.
/// </summary>
/// <param name="Total">입력으로 받은 전체 행 수다.</param>
/// <param name="Created">실제로 생성된 직원 수다.</param>
/// <param name="Failed">생성에 실패한 행 수다.</param>
/// <param name="Errors">행 단위 오류 목록이다.</param>
public sealed record BulkCreateEmployeesResult(
    int Total,
    int Created,
    int Failed,
    IReadOnlyList<BulkCreateEmployeesError> Errors);
