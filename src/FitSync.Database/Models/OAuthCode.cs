namespace FitSync.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("oauth_codes")]
public class OAuthCode
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [Column("client_id")]
    public Guid ClientId { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("redirect_uri")]
    public string RedirectUri { get; set; } = string.Empty;

    [Required]
    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("used_at")]
    public DateTime? UsedAt { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    public OAuthClient Client { get; set; } = null!;
    public User User { get; set; } = null!;
}
