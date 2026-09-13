namespace FitSync.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

[Table("users")]
[Index(nameof(Username), IsUnique = true)]
[Index(nameof(Email), IsUnique = true, Name = "idx_users_email")]
public class User
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    [Column("email_hash")]
    public string EmailHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Required]
    [Column("is_verified")]
    public bool IsVerified { get; set; } = false;

    [MaxLength(255)]
    [Column("verification_token")]
    public string? VerificationToken { get; set; }

    [Column("verification_token_expires_at")]
    public DateTime? VerificationTokenExpiresAt { get; set; }

    [MaxLength(255)]
    [Column("reset_token")]
    public string? ResetToken { get; set; }

    [Column("reset_token_expires_at")]
    public DateTime? ResetTokenExpiresAt { get; set; }

    // Navigation property
    [JsonIgnore]
    public ICollection<Activity> Activities { get; set; } = [];
}
