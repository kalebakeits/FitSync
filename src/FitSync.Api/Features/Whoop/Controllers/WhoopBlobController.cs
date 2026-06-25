namespace FitSync.Api.Features.Whoop.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class WhoopBlobController(IConfiguration configuration, ILogger<WhoopBlobController> logger)
    : ControllerBase
{
    private readonly IConfiguration configuration = configuration;
    private readonly ILogger<WhoopBlobController> logger = logger;

    [HttpPost]
    public async Task<IActionResult> UploadBlob(
        [FromHeader(Name = "X-Device-Id")] string deviceId,
        [FromHeader(Name = "X-Packet-Type")] string packetType,
        [FromHeader(Name = "X-Sequence-Start")] long sequenceStart,
        [FromHeader(Name = "X-Packet-Count")] int packetCount
    )
    {
        if (!this.IsAuthorized())
            return this.Unauthorized();

        if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(packetType))
            return this.BadRequest("X-Device-Id and X-Packet-Type headers are required.");

        string blobRoot = this.configuration["Whoop:BlobPath"] ?? "/data/whoop-blobs";
        string deviceDir = Path.Combine(blobRoot, deviceId);
        Directory.CreateDirectory(deviceDir);

        string fileName =
            $"{packetType}_{sequenceStart}_{DateTimeOffset.UtcNow:yyyyMMdd_HHmmss}.bin";
        string filePath = Path.Combine(deviceDir, fileName);

        using FileStream fs = System.IO.File.Create(filePath);
        await this.Request.Body.CopyToAsync(fs);
        long receivedBytes = fs.Length;

        this.logger.LogInformation(
            "WhoopBlob received: device={DeviceId} type={PacketType} seqStart={SeqStart} count={Count} bytes={Bytes} file={File}",
            deviceId,
            packetType,
            sequenceStart,
            packetCount,
            receivedBytes,
            fileName
        );

        return this.Ok(new { received_bytes = receivedBytes });
    }

    private bool IsAuthorized()
    {
        string? expectedKey = this.configuration["Whoop:Auth:Key"];
        string? expectedValue = this.configuration["Whoop:Auth:Value"];

        if (string.IsNullOrEmpty(expectedKey) || string.IsNullOrEmpty(expectedValue))
        {
            this.logger.LogError("Whoop basic auth not configured.");
            return false;
        }

        if (!this.Request.Headers.TryGetValue(expectedKey, out var provided))
            return false;

        return provided == expectedValue;
    }
}
