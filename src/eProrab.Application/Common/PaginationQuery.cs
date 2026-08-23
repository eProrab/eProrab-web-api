namespace eProrab.Application.Common;

/// <summary>
/// Common paging/search/sort parameters accepted by every admin "list" endpoint
/// (items, categories, users). Bound from query-string parameters.
/// </summary>
public class PaginationQuery
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;

    public int? Page { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value is < 1 or > MaxPageSize ? 20 : value;
    }

    /// <summary>Free-text search applied to name/email/sku depending on the resource.</summary>
    public string? Search { get; set; }

    public string? SortBy { get; set; }

    public bool? SortDescending { get; set; } = false;
}

