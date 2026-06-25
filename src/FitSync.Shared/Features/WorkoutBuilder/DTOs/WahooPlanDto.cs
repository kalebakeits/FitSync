namespace FitSync.Shared.Features.WorkoutBuilder.DTOs;

using System.Text.Json.Serialization;

public record WahooPlanDto(
    [property: JsonPropertyName("header")] WahooPlanHeader Header,
    [property: JsonPropertyName("intervals")] WahooPlanInterval[] Intervals
);

public record WahooPlanHeader(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("workout_type_family")] int WorkoutTypeFamily,
    [property: JsonPropertyName("workout_type_location")] int WorkoutTypeLocation,
    [property: JsonPropertyName("ftp")] int Ftp
);

public record WahooPlanInterval(
    [property: JsonPropertyName("targets")] WahooPlanTarget[] Targets,
    [property: JsonPropertyName("exit_trigger_type")] string ExitTriggerType,
    [property: JsonPropertyName("exit_trigger_value")] int ExitTriggerValue,
    [property: JsonPropertyName("intensity_type")] string IntensityType,
    [property: JsonPropertyName("name")] string? Name = null
);

public record WahooPlanTarget(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("low")] double Low,
    [property: JsonPropertyName("high")] double High
);
