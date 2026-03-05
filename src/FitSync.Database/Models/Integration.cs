namespace FitSync.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

[Table("integrations")]
[Index(nameof(UserId), nameof(ServiceType), IsUnique = true)]
[Index(nameof(LookupValue))]
public class Integration
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
    [Column("auth_data", TypeName = "text")]
    public string AuthData { get; set; } = string.Empty;

    [Required]
    [Column("failure_count")]
    public int FailureCount { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [MaxLength(255)]
    [Column("lookup_value")]
    public string? LookupValue { get; set; }

    [JsonIgnore]
    public User User { get; set; } = null!;

    public FetcherConfig? FetcherConfig { get; set; }
}
