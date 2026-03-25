using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrandService;

[Table("brand_events")]
public class BrandEvent
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("brand_id")]
    public int BrandId { get; set; }

    [Column("event_type")]
    public string EventType { get; set; } = string.Empty;

    [Column("payload", TypeName = "jsonb")]
    public string Payload { get; set; } = string.Empty; // JSON string

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}