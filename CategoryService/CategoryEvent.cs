using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CategoryService;

[Table("category_events")]
public class CategoryEvent
{
    [Key]
    public int Id { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("event_type")]
    public string EventType { get; set; } = string.Empty;

    [Column("payload")]
    public string Payload { get; set; } = string.Empty; // JSON string

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}