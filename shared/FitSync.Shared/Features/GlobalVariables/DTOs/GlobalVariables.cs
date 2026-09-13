namespace FitSync.Shared.Features.GlobalVariables.DTOs;

/// <param name="Roles">
/// The roles this process advertises. A merged process serves several roles and heartbeats once
/// per role, so an instance id here is per-role rather than per-process.
/// </param>
/// <param name="Instance">
/// The process identity used for work claiming and worker naming. For a process that serves an
/// uploader role this is the uploader's instance id.
/// </param>
public sealed record GlobalVariables(
    IReadOnlyList<HeartbeatRole> Roles,
    string Instance,
    string HostName,
    int HeartbeatIntervalMinutes,
    string ServiceName
);
