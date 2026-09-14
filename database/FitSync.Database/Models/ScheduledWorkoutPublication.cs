namespace FitSync.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FitSync.Database.Enums;

[Table("scheduled_workout_publications")]
public class ScheduledWorkoutPublication
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("scheduled_workout_id")]
    public Guid ScheduledWorkoutId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("service_type")]
    public string ServiceType { get; set; } = string.Empty;

    [Column("service_metadata", TypeName = "jsonb")]
    public string? ServiceMetadata { get; set; }

    [Required]
    [Column("published_at")]
    public DateTime PublishedAt { get; set; }

    [Required]
    [Column("status")]
    public PublicationStatus Status { get; set; }

    [Required]
    [Column("attempt_count")]
    public int AttemptCount { get; set; }

    [Column("last_error")]
    public string? LastError { get; set; }

    public ScheduledWorkout ScheduledWorkout { get; set; } = null!;
}
