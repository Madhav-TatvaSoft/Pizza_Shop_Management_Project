namespace DAL.ViewModels;

public class PaginationViewModel<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }

    //     page.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
    public int StartItem => TotalCount == 0 ? 0 : ((PageNumber - 1) * PageSize + 1);
    public int EndItem => Math.Min(PageNumber * PageSize , TotalCount);
    public bool HasNextPage => EndItem < TotalCount;
    public bool HasPreviousPage  => PageNumber > 1;

    public PaginationViewModel(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items.ToList();
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
