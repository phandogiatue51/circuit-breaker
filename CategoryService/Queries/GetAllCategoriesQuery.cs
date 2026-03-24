namespace CategoryService.Queries
{
    public class GetAllCategoriesQuery
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }
}
