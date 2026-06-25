namespace FitSync.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("oauth_clients")]
public class OAuthClient
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    [Column("client_secret_hash")]
    public string ClientSecretHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("redirect_uris")]
    public string[] RedirectUris { get; set; } = [];

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
