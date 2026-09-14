namespace FitSync.Shared.Features.GlobalVariables.DTOs;

using FitSync.Database.Enums;

public sealed record HeartbeatRole(string InstanceId, ServiceType ServiceType);
