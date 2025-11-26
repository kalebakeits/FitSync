namespace FitSync.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using FitSync.Database.Enums;
using Microsoft.EntityFrameworkCore;

[Table("activities")]
[Index(nameof(UserId), Name = "idx_activities_user_id")]
[Index(nameof(Status), Name = "idx_activities_status")]
[Index(nameof(ClaimedBy), Name = "idx_activities_claimed_by")]
[Index(nameof(ActivityDate), Name = "idx_activities_activity_date")]
[Index(nameof(UserId), nameof(ExternalActivityId), nameof(Source), IsUnique = true)]
public class Activity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("user_id")]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("external_activity_id")]
    public string ExternalActivityId { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Column("source")]
    public string Source { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Column("status")]
    public ActivityStatus Status { get; set; }

    // File info
    [MaxLength(500)]
    [Column("original_file_name")]
    public string? OriginalFileName { get; set; }

    [Column("fit_file_data")]
    public byte[]? FitFileData { get; set; }

    [Column("file_size_bytes")]
    public long? FileSizeBytes { get; set; }

    // Processing metadata
    [Column("claimed_by")]
    public string? ClaimedBy { get; set; }

    [Column("claimed_at")]
    public DateTime? ClaimedAt { get; set; }

    [Column("processing_started_at")]
    public DateTime? ProcessingStartedAt { get; set; }

    [Column("processing_completed_at")]
    public DateTime? ProcessingCompletedAt { get; set; }

    // Activity metadata
    [Required]
    [Column("activity_date")]
    public DateTime ActivityDate { get; set; }

    [MaxLength(500)]
    [Column("activity_name")]
    public string? ActivityName { get; set; }

    [Column("activity_metadata", TypeName = "jsonb")]
    public string? ActivityMetadata { get; set; } // JSON

    // Error tracking
    [Column("retry_count")]
    public int RetryCount { get; set; } = 0;

    [Column("last_error")]
    public string? LastError { get; set; }

    [Column("last_error_at")]
    public DateTime? LastErrorAt { get; set; }

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
