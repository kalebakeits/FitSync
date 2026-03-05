namespace FitSync.Shared.Features.GlobalVariables.DTOs;

using FitSync.Database.Enums;

public sealed record GlobalVariables(
    string Instance,
    string HostName,
    int HeartbeatIntervalMinutes,
    ServiceType ServiceType,
    string ServiceName
);
