namespace FitSync.Mock.Fetcher.Services;

using System.Text;
using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.DTOs;
using FitSync.Shared.Features.Fetcher.Services;
using Microsoft.Extensions.Logging;

public class MockFetcherService(ILogger<MockFetcherService> logger) : IFetcherService
{
    private readonly ILogger<MockFetcherService> logger = logger;

    public async Task<List<FetchedActivity>> GetActivitiesAsync(
        User user,
        CancellationToken cancellationToken
    )
    {
        byte[] fitData = this.GetMockFitFileData();
        Guid guid = Guid.NewGuid();
        FetchedActivity activity =
            new(
                ExternalActivityId: $"zwift-{guid}",
                Source: d,
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
