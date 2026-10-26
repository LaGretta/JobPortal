namespace JobPortal.Application.DTO;

public class PageRequest
{
    private const int MaxPageSize = 50;
    
    //по 20 записів на сторінці за замовчуванням
    private int _pageSize = 20;
    
    //якщо клієнт не сказав яку сторінку хоче беремо першу
    private int _page = 1;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => 1,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }

    public int Skip => (Page - 1) * PageSize;
}
public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    
    //перевірка є настпна сторінка 
    public bool HasNext => Page < TotalPages;
}