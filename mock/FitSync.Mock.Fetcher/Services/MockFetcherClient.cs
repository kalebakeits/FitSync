namespace FitSync.Mock.Fetcher.Services;

using System.Text;
using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.DTOs;
using FitSync.Shared.Features.Fetcher.Services;
using Microsoft.Extensions.Logging;

public class MockFetcherClient(ILogger<MockFetcherClient> logger) : IFetcherClient
{
    private readonly ILogger<MockFetcherClient> logger = logger;

    public async Task<List<FetchedActivity>> GetActivitiesAsync(
        Integration integration,
        int lookbackDays,
        CancellationToken cancellationToken = default
    )
    {
        byte[] fitData = this.GetMockFitFileData();
        Guid guid = Guid.NewGuid();
        FetchedActivity activity =
            new(
                ExternalActivityId: $"mock-{guid}",
                Source: "Mock",
                ActivityDate: DateTime.UtcNow.AddHours(-1),
                FileName: $"mock-activity-{guid}.fit",
                FitFileData: fitData,
                Metadata: new Dictionary<string, string> { { "mock", "true" }, { "test", "data" } }
            );

        return [activity];
    }

    private byte[] GetMockFitFileData()
    {
        string testDataPath = Path.Combine(AppContext.BaseDirectory, "TestData");
        string[] fitFiles = Directory.GetFiles(testDataPath, "*.fit");

        if (fitFiles.Length == 0)
        {
            this.logger.LogWarning("No FIT files found in TestData directory");
            return Encoding.UTF8.GetBytes("NO_FIT_FILES_AVAILABLE");
        }

        string selectedFile = fitFiles[Random.Shared.Next(fitFiles.Length)];
        byte[] fitData = File.ReadAllBytes(selectedFile);

        this.logger.LogInformation(
            "Loaded FIT file: {FileName} ({Bytes} bytes)",
            Path.GetFileName(selectedFile),
            fitData.Length
        );

        return fitData;
    }
}
