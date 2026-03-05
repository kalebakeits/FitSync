namespace FitSync.Shared.Features.RateLimiting;

public record RateLimit(int WindowMinutes, int Cap);
