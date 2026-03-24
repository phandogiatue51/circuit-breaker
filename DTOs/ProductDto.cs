namespace DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string? Origin { get; set; }
        public string? Material { get; set; }
        public bool IsActive { get; set; }

        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;

        // Categories (multiple)
        public List<CategoryInfoDto> Categories { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt
        {
            get; set;
        }

        public class CategoryInfoDto
        {
            public int CategoryId { get; set; }
            public string CategoryName { get; set; } = string.Empty;
        }
    }
}
