namespace Core.Common
{
    public class Filter
    {
        public string? Keyword { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public int? Page { get; set; } = 1;

        public int? PageSize { get; set; } = 20;

        public IEnumerable<FilterField>? Filters { get; set; }
    }

    public class FilterField
    {
        public string Code { get; set; } = "";

        public string Value { get; set; } = "";
    }
}
