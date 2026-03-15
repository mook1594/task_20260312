namespace EmployeeContacts.Api.Models;

/// <summary>
/// 페이징된 조회 결과와 네비게이션 링크를 표현한다.
/// </summary>
/// <typeparam name="T">목록 항목 타입이다.</typeparam>
public sealed record PagedResponse<T>
{
    /// <summary>
    /// 현재 페이지의 항목 목록이다.
    /// </summary>
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>
    /// 현재 페이지 번호다.
    /// </summary>
    public required int Page { get; init; }

    /// <summary>
    /// 페이지당 요청된 항목 수다.
    /// </summary>
    public required int PageSize { get; init; }

    /// <summary>
    /// 전체 항목 수다.
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// 전체 페이지 수다.
    /// </summary>
    public required int TotalPages { get; init; }

    /// <summary>
    /// 다음/이전 페이지 네비게이션 링크다.
    /// </summary>
    public required PagedLinks Links { get; init; }
}

/// <summary>
/// 페이지네이션 네비게이션 링크를 표현한다.
/// </summary>
public sealed record PagedLinks
{
    /// <summary>
    /// 다음 페이지로 이동하는 URL이다. 마지막 페이지면 null이다.
    /// </summary>
    public string? Next { get; init; }

    /// <summary>
    /// 이전 페이지로 이동하는 URL이다. 첫 페이지면 null이다.
    /// </summary>
    public string? Prev { get; init; }
}
