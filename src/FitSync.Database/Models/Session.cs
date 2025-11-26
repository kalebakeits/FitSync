namespace FitSync.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[Table("sessions")]
[Index(nameof(SessionId), IsUnique = true)]
[Index(nameof(UserId))]
public class Session
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("session_id")]
    public string SessionId { get; set; } = string.Empty;

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    // Navigation property
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
