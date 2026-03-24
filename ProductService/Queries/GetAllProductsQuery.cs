namespace ProductService.Queries
{
    public class GetAllProductsQuery
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }
}
