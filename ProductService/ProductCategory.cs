using System.ComponentModel.DataAnnotations;

namespace ProductService
{
    public class ProductCategory
    {
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int CategoryId { get; set; }

        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;
    }
}