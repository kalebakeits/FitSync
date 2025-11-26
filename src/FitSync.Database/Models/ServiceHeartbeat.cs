namespace FitSync.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FitSync.Database.Enums;
using Microsoft.EntityFrameworkCore;

[Table("service_heartbeats")]
[Index(nameof(InstanceId), IsUnique = true)]
[Index(nameof(ServiceType), Name = "idx_service_heartbeats_service_type")]
[Index(
    nameof(ServiceType),
    nameof(LastHeartbeatAt),
    Name = "idx_service_heartbeats_last_heartbeat"
)]
public class ServiceHeartbeat
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("service_type")]
    public ServiceType ServiceType { get; set; }

    [Required]
    [Column("instance_id")]
    public string? InstanceId { get; set; }

    [MaxLength(255)]
    [Column("hostname")]
    public string? Hostname { get; set; }

    [Required]
    [Column("last_heartbeat_at")]
    public DateTime LastHeartbeatAt { get; set; }

    [Column("processed_count")]
    public int ProcessedCount { get; set; } = 0;

    [Column("error_count")]
    public int ErrorCount { get; set; } = 0;

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
