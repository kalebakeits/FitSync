namespace FitSync.Shared.Features.WorkoutBuilder.DTOs;

using System.Text.Json.Serialization;
using Dunet;
using Dynastream.Fit;

[Union]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(Step), "step")]
[JsonDerivedType(typeof(SwimStep), "swimStep")]
[JsonDerivedType(typeof(Repeat), "repeat")]
public partial record WorkoutItem
{
    public partial record Step(
        [property: JsonPropertyName("durationType")]
            WktStepDuration DurationType = WktStepDuration.Open,
        [property: JsonPropertyName("durationValue")] uint? DurationValue = null,
        [property: JsonPropertyName("targetType")] WktStepTarget TargetType = WktStepTarget.Open,
        [property: JsonPropertyName("targetLow")] uint? TargetLow = null,
        [property: JsonPropertyName("targetHigh")] uint? TargetHigh = null,
        [property: JsonPropertyName("targetZone")] int? TargetZone = null,
        [property: JsonPropertyName("intensity")] Intensity Intensity = Intensity.Active,
        [property: JsonPropertyName("name")] string? Name = null
    );

    public partial record SwimStep(
        [property: JsonPropertyName("distance")] float Distance,
        [property: JsonPropertyName("swimStroke")] SwimStroke SwimStroke = SwimStroke.Invalid,
        [property: JsonPropertyName("equipment")] WorkoutEquipment? Equipment = null,
        [property: JsonPropertyName("intensity")] Intensity Intensity = Intensity.Active,
        [property: JsonPropertyName("name")] string? Name = null
    );

    public partial record Repeat(
        [property: JsonPropertyName("steps")] WorkoutItem[] Steps,
        [property: JsonPropertyName("repeatCount")] uint RepeatCount
    );
}
