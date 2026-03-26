namespace ProductService.Commands
{
    public class UpdateProductCommand
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Origin { get; set; }
        public string? Material { get; set; }
        public bool? IsActive { get; set; }
        public int? BrandId { get; set; }
        public List<int>? CategoryIds { get; set; }
    }
}
