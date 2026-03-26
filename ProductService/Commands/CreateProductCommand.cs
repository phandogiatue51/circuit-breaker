namespace ProductService.Commands
{
    public class CreateProductCommand
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Origin { get; set; }
        public string? Material { get; set; }
        public int BrandId { get; set; }
        public List<int> CategoryIds { get; set; } = new List<int>();
    }
}