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

    [Required]
    [Column("scheduled_date")]
    public DateOnly ScheduledDate { get; set; }

    [Column("planned_duration_seconds")]
    public int? PlannedDurationSeconds { get; set; }

    [Column("pending_publish_at")]
    public DateTime? PendingPublishAt { get; set; }

    [Column("publish_claimed_at")]
    public DateTime? PublishClaimedAt { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public Workout Workout { get; set; } = null!;
    public List<ScheduledWorkoutPublication> Publications { get; set; } = [];
}
