using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService
{
    [Table("product_events")]
    public class ProductEvent
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("event_type")]
        public string EventType { get; set; } = string.Empty;

        [Column("payload", TypeName = "nvarchar(max)")]  
        public string Payload { get; set; } = string.Empty; // JSON string

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}   