namespace Core.Common
{
    /// <summary>
    /// Phân trang hệ thống
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class PagedResult<TModel> where TModel : class
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<TModel> Items { get; set; } = new();

        public PagedResult() { }

        public PagedResult(IEnumerable<TModel> items, int totalRecord, int? page = 1, int? pageSize = 30)
        {
            CurrentPage = page.Value;
            TotalPages = totalRecord % pageSize.Value == 0 ? totalRecord / pageSize.Value : (totalRecord / pageSize.Value) + 1;
            PageSize = pageSize.Value;
            TotalCount = totalRecord;
            Items = items.ToList();
        }
    }

    public class PaginationHeader
    {
        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        public PaginationHeader(int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            CurrentPage = currentPage;
            ItemsPerPage = itemsPerPage;
            TotalItems = totalItems;
            TotalPages = totalPages;
        }
    }
}
