namespace KhatiExtendedEF.Model
{
    public class PaginationResponseModel<T> where T : class
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public List<T>? Data { get; set; }   
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
