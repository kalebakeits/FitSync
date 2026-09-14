namespace FitSync.Database.Enums;

public enum ServiceType
{
    ZwiftFetcher,
    WahooFetcher,
    GarminUploader,
    MockFetcher,
    AmazonS3,
    // Appended, never inserted: ServiceType is persisted as its underlying int,
    // so reordering renumbers existing rows.
    GarminFetcher
}
