namespace FitSync.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

[Table("processed_activities")]
[Index(nameof(UserId), nameof(Source), Name = "idx_processed_activities_user_source")]
[Index(nameof(UserId), nameof(ExternalActivityId), nameof(Source), IsUnique = true)]
public class ProcessedActivity
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
    [Column("fetched_at")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime FetchedAt { get; set; }

    // Navigation property
    [JsonIgnore]
    public User User { get; set; } = null!;
}
