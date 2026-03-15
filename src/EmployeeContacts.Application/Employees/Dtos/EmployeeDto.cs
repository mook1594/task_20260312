namespace EmployeeContacts.Application.Employees.Dtos;

/// <summary>
/// 외부 API에 반환되는 직원 연락처 정보다.
/// </summary>
/// <param name="Id">직원 식별자다.</param>
/// <param name="Name">직원 이름이다.</param>
/// <param name="Email">정규화된 이메일 주소다.</param>
/// <param name="Tel">숫자만 저장된 전화번호다.</param>
/// <param name="Joined">입사일이다.</param>
public sealed record EmployeeDto(
    Guid Id,
    string Name,
    string Email,
    string Tel,
    DateOnly Joined);
