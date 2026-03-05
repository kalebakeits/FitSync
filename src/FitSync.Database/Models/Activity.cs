namespace FitSync.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

[Table("activities")]
[Index(nameof(UserId), Name = "idx_activities_user_id")]
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

    // File info
    [MaxLength(500)]
    [Column("original_file_name")]
    public string? OriginalFileName { get; set; }

    [Column("fit_file_data")]
    public byte[]? FitFileData { get; set; }

    [Column("file_size_bytes")]
    public long? FileSizeBytes { get; set; }

    // Activity metadata
    [Required]
    [Column("activity_date")]
    public DateTime ActivityDate { get; set; }

    [MaxLength(500)]
    [Column("activity_name")]
    public string? ActivityName { get; set; }

    [Column("activity_metadata", TypeName = "jsonb")]
    public string? ActivityMetadata { get; set; }

    // Soft delete
    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [JsonIgnore]
    public User User { get; set; } = null!;

    public ICollection<ActivityUploadStatus> UploadStatuses { get; set; } = [];
}
