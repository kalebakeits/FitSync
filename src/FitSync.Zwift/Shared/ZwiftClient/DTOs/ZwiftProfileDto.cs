namespace FitSync.Zwift.Shared.ZwiftClient.DTOs;

using System.Text.Json.Serialization;

public record ZwiftProfileDto([property: JsonPropertyName("id")] long Id);
