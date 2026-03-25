namespace CategoryService.Queries
{
    public class GetCategoryQuery
    {
        public int Id { get; set; }
    }
    public class GetCategoriesByIdsQuery
    {
        public List<int> Ids { get; set; } = new List<int>();
    }
}
