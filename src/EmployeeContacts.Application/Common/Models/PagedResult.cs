namespace EmployeeContacts.Application.Common.Models;

public sealed record PagedResult<T>
{
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

    public IReadOnlyList<T> Items { get; }

    public int Page { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages { get; }
}
