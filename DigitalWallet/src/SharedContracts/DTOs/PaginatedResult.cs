namespace SharedContracts.DTOs;

/// <summary>
/// Generic paginated result wrapper for list endpoints.
/// </summary>
public class PaginatedResult<T>
{
    /// <summary>
    /// The collection of items on the current page.
    /// </summary>
    public List<T> Items { get; set; } = new();
    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    public int Page { get; set; }
    /// <summary>
    /// The maximum number of items per page.
    /// </summary>
    public int PageSize { get; set; }
    /// <summary>
    /// The total number of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }
    /// <summary>
    /// The total number of pages calculated from TotalCount and PageSize.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    /// <summary>
    /// Indicates whether there is a page after the current page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
    /// <summary>
    /// Indicates whether there is a page before the current page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;
}
