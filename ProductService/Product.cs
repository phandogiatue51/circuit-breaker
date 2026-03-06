using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService
{
    public class Product

    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string? Origin { get; set; }

        public string? Material { get; set; }
        public bool IsActive { get; set; } = true;

        [Required]
        public int BrandId { get; set; }

        [MaxLength(100)]
        public string BrandName { get; set; } = string.Empty;

        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}