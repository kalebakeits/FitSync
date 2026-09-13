namespace FitSync.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("scheduled_workouts")]
public class ScheduledWorkout
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("workout_id")]
    public Guid WorkoutId { get; set; }

    [MaxLength(50)]
    [Column("service_type")]
    public string? ServiceType { get; set; }

    [Required]
    [Column("scheduled_date")]
    public DateOnly ScheduledDate { get; set; }

    [Column("service_metadata", TypeName = "jsonb")]
    public string? ServiceMetadata { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public Workout Workout { get; set; } = null!;
}
