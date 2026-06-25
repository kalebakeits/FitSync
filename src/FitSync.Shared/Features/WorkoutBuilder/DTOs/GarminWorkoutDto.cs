namespace FitSync.Shared.Features.WorkoutBuilder.DTOs;

using System.Text.Json.Serialization;

public record GarminWorkoutDto(
    [property: JsonPropertyName("workoutName")] string WorkoutName,
    [property: JsonPropertyName("sportType")] GarminSportType SportType,
    [property: JsonPropertyName("workoutSegments")] GarminWorkoutSegment[] WorkoutSegments
);

public record GarminSportType(
    [property: JsonPropertyName("sportTypeKey")] string SportTypeKey,
    [property: JsonPropertyName("sportTypeId")] int SportTypeId
);

public record GarminWorkoutSegment(
    [property: JsonPropertyName("segmentOrder")] int SegmentOrder,
    [property: JsonPropertyName("sportType")] GarminSportType SportType,
    [property: JsonPropertyName("workoutSteps")] GarminWorkoutStep[] WorkoutSteps
);

public record GarminWorkoutStep(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("stepOrder")] int StepOrder,
    [property: JsonPropertyName("stepType")] GarminStepType StepType,
    [property:
        JsonPropertyName("childStepId"),
        JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)
    ]
        int? ChildStepId,
    [property:
        JsonPropertyName("endCondition"),
        JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)
    ]
        GarminEndCondition? EndCondition,
    [property:
        JsonPropertyName("endConditionValue"),
        JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)
    ]
        double? EndConditionValue,
    [property:
        JsonPropertyName("targetType"),
        JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)
    ]
        GarminTargetType? TargetType,
    [property:
        JsonPropertyName("targetValueOne"),
        JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)
    ]
        double? TargetValueOne,
    [property:
        JsonPropertyName("targetValueTwo"),
        JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)
    ]
        double? TargetValueTwo,
    [property:
        JsonPropertyName("numberOfIterations"),
        JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)
    ]
        int? NumberOfIterations,
    [property:
        JsonPropertyName("workoutSteps"),
        JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)
    ]
        GarminWorkoutStep[]? WorkoutSteps,
    [property:
        JsonPropertyName("smartRepeat"),
        JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)
    ]
        bool? SmartRepeat
);

public record GarminStepType(
    [property: JsonPropertyName("stepTypeKey")] string StepTypeKey,
    [property: JsonPropertyName("stepTypeId")] int StepTypeId
);

public record GarminEndCondition(
    [property: JsonPropertyName("conditionTypeKey")] string ConditionTypeKey,
    [property: JsonPropertyName("conditionTypeId")] int ConditionTypeId
);

public record GarminTargetType(
    [property: JsonPropertyName("workoutTargetTypeKey")] string WorkoutTargetTypeKey,
    [property: JsonPropertyName("workoutTargetTypeId")] int WorkoutTargetTypeId
);
