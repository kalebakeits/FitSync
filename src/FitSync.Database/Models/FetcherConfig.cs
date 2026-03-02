namespace FitSync.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

[Table("fetcher_configs")]
[Index(nameof(IntegrationId), IsUnique = true)]
[Index(nameof(NextFetchTime))]
public class FetcherConfig
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("integration_id")]
    [ForeignKey(nameof(Integration))]
    public Guid IntegrationId { get; set; }

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

    [JsonIgnore]
    public Integration Integration { get; set; } = null!;
}
