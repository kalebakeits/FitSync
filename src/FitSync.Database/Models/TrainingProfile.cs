namespace FitSync.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

[Table("training_profiles")]
public class TrainingProfile
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    // Cycling
    [Column("ftp_watts")]
    public int? FtpWatts { get; set; }

    [Column("cycling_threshold_hr")]
    public int? CyclingThresholdHr { get; set; }

    [Column("cycling_max_hr")]
    public int? CyclingMaxHr { get; set; }

    // Running
    [Column("running_threshold_hr")]
    public int? RunningThresholdHr { get; set; }

    [Column("running_max_hr")]
    public int? RunningMaxHr { get; set; }

    [Column("running_threshold_pace_seconds")]
    public int? RunningThresholdPaceSeconds { get; set; }

    // Swimming
    [Column("pool_length_metres")]
    public float? PoolLengthMetres { get; set; }

    [Column("swim_threshold_hr")]
    public int? SwimThresholdHr { get; set; }

    [Column("swim_css_seconds")]
    public int? SwimCssSeconds { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonIgnore]
    public User User { get; set; } = null!;
}
