namespace InvoicesService.Application.DTOs.Responses;

public class PagedResult<T>
{
    public List<T> Data { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public PagedResult(List<T> data, int currentPage, int totalCount, int pageSize)
    {
        Data = data;
        CurrentPage = currentPage;
        TotalCount = totalCount;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}
