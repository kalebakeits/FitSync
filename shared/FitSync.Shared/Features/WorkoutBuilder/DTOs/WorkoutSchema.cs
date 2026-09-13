namespace FitSync.Shared.Features.WorkoutBuilder.DTOs;

using System.Text.Json.Serialization;
using Dunet;
using Dynastream.Fit;

[Union]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(Default), "default")]
[JsonDerivedType(typeof(PoolSwim), "poolSwim")]
public partial record WorkoutSchema
{
    public partial record Default(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("sport")] Sport Sport,
        [property: JsonPropertyName("subSport")] SubSport? SubSport,
        [property: JsonPropertyName("items")] WorkoutItem[] Items,
        [property: JsonPropertyName("skipLastRest")] bool SkipLastRest,
        [property: JsonPropertyName("description")] string? Description = null
    ) : IWorkoutMeta;

    public partial record PoolSwim(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("sport")] Sport Sport,
        [property: JsonPropertyName("subSport")] SubSport? SubSport,
        [property: JsonPropertyName("items")] WorkoutItem[] Items,
        [property: JsonPropertyName("skipLastRest")] bool SkipLastRest,
        [property: JsonPropertyName("poolLength")] float PoolLength,
        [property: JsonPropertyName("poolLengthUnit")] DisplayMeasure PoolLengthUnit,
        [property: JsonPropertyName("description")] string? Description = null
    ) : IWorkoutMeta;
}
