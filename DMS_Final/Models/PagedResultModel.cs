namespace DMS_Final.Models
{
    public class PagedResultModel<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    }
}
