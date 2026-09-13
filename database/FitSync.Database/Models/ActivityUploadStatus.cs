namespace FitSync.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FitSync.Database.Enums;
using Microsoft.EntityFrameworkCore;

[Table("activity_upload_statuses")]
[Index(nameof(ActivityId), Name = "idx_activity_upload_statuses_activity_id")]
[Index(nameof(Status), Name = "idx_activity_upload_statuses_status")]
[Index(nameof(ClaimedBy), Name = "idx_activity_upload_statuses_claimed_by")]
public class ActivityUploadStatus
{
    [Column("activity_id")]
    public Guid ActivityId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("destination_service_type")]
    public string DestinationServiceType { get; set; } = null!;

    [Column("status")]
    public ActivityStatus Status { get; set; } = ActivityStatus.Pending;

    [Column("claimed_by")]
    public string? ClaimedBy { get; set; }

    [Column("claimed_at")]
    public DateTime? ClaimedAt { get; set; }

    [Column("processing_started_at")]
    public DateTime? ProcessingStartedAt { get; set; }

    [Column("processing_completed_at")]
    public DateTime? ProcessingCompletedAt { get; set; }

    [Column("retry_count")]
    public int RetryCount { get; set; }

    [Column("last_error")]
    public string? LastError { get; set; }

    [Column("last_error_at")]
    public DateTime? LastErrorAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public Activity Activity { get; set; } = null!;
}
