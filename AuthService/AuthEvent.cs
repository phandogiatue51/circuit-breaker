using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService;

[Table("auth_events")]
public class AuthEvent
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("auth_id")]
    public int AuthId { get; set; }

    [Column("event_type")]
    public string EventType { get; set; } = string.Empty;

    [Column("payload", TypeName = "jsonb")]
    public string Payload { get; set; } = string.Empty; // JSON string

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}