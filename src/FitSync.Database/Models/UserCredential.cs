namespace FitSync.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

[Table("user_credentials")]
[Index(nameof(UserId), nameof(ServiceType), IsUnique = true)]
public class UserCredential
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("user_id")]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("service_type")]
    public string ServiceType { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    [Column("password")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Column("failure_count")]
    public int FailureCount { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    [JsonIgnore]
    public User User { get; set; } = null!;
}

public static class ServiceTypes
{
    public const string Zwift = "Zwift";
    public const string Garmin = "Garmin";
}
