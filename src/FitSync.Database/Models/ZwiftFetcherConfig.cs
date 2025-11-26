namespace FitSync.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

[Table("zwift_fetcher_configs")]
[Index(nameof(UserId), IsUnique = true)]
[Index(nameof(NextFetchTime))]
public class ZwiftFetcherConfig
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("user_id")]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    [Column("access_token", TypeName = "text")]
    public string? AccessToken { get; set; }

    [Column("refresh_token", TypeName = "text")]
    public string? RefreshToken { get; set; }

    [MaxLength(100)]
    [Column("profile_id")]
    public string? ProfileId { get; set; }

    [Column("next_fetch_time")]
    public DateTime? NextFetchTime { get; set; }

    [Column("worker_lock_id")]
    public string? WorkerLockId { get; set; }

    [Column("lock_expiry")]
    public DateTime? LockExpiry { get; set; }

    [Required]
    [Column("fetch_interval_minutes")]
    public int FetchIntervalMinutes { get; set; } = 60;

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
