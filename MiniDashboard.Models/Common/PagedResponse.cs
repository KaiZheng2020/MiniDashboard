namespace MiniDashboard.Models.Common;

public class PagedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public PagedResponse()
    {
    }

    public PagedResponse(List<T> data, int page, int pageSize, int totalCount)
    {
        Data = data;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public static PagedResponse<T> Create(List<T> data, int page, int pageSize, int totalCount)
    {
        return new PagedResponse<T>(data, page, pageSize, totalCount);
    }
}

