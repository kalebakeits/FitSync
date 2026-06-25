namespace FitSync.Api.Features.Wahoo.DTOs;

using System.Text.Json.Serialization;

public record WahooUserResponse([property: JsonPropertyName("id")] long Id);
