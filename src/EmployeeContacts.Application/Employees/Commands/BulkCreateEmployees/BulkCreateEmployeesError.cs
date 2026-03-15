namespace EmployeeContacts.Application.Employees.Commands.BulkCreateEmployees;

/// <summary>
/// 직원 일괄 등록 실패 행의 오류 정보다.
/// </summary>
/// <param name="Row">실패한 데이터 행 번호다. 1부터 시작한다.</param>
/// <param name="Field">오류 대상 필드명이다.</param>
/// <param name="Code">오류 코드다.</param>
/// <param name="Message">사용자에게 노출할 오류 메시지다.</param>
public sealed record BulkCreateEmployeesError(
    int Row,
    string Field,
    string Code,
    string Message);
