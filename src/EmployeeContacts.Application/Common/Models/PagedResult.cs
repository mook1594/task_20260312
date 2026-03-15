namespace EmployeeContacts.Application.Common.Models;

/// <summary>
/// 페이징된 조회 결과를 표현한다.
/// </summary>
/// <typeparam name="T">목록 항목 타입이다.</typeparam>
public sealed record PagedResult<T>
{
    /// <summary>
    /// 페이징 결과를 초기화한다.
    /// </summary>
    public PagedResult(
        IReadOnlyList<T> items,
        int page,
        int pageSize,
        int totalCount,
        int totalPages)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = totalCount == 0 ? 0 : totalPages;
    }

    /// <summary>
    /// 현재 페이지의 항목 목록이다.
    /// </summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    /// 현재 페이지 번호다.
    /// </summary>
    public int Page { get; }

    /// <summary>
    /// 페이지당 요청된 항목 수다.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// 전체 항목 수다.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// 전체 페이지 수다.
    /// </summary>
    public int TotalPages { get; }
}
