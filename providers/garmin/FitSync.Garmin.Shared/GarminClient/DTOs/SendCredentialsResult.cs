namespace FitSync.Garmin.Shared.GarminClient.DTOs;

public record SendCredentialsResult(
    bool WasRedirected,
    string RedirectedTo,
    string RawResponseBody
);
