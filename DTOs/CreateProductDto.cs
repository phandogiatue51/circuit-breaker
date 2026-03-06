using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public class CreateProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string? Origin { get; set; }
        public string? Material { get; set; }

        [Required]
        public int BrandId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one category is required")]
        public List<int> CategoryIds { get; set; } = new List<int>();
    }
}
