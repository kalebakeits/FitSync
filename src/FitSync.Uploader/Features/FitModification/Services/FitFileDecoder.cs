namespace FitSync.Uploader.Features.FitModification.Services;

using Dynastream.Fit;

public class FitFileDecoder(ILogger<FitFileDecoder> logger) : IFitFileDecoder
{
    private readonly ILogger<FitFileDecoder> logger = logger;

    public List<Mesg> DecodeMessages(byte[] fitFileData)
    {
        using MemoryStream inputStream = new(fitFileData);
        Decode decoder = new();

        List<Mesg> messages = [];

        decoder.MesgEvent += (sender, args) =>
        {
            messages.Add(args.mesg);
        };

        bool isValid = decoder.IsFIT(inputStream);
        if (!isValid)
        {
            this.logger.LogWarning("Invalid FIT file. Don't lie about your training data again!");
            throw new InvalidOperationException("Invalid FIT file");
        }
        decoder.Read(inputStream);

        this.logger.LogDebug(
            "Decoded {Count} messages from FIT file. Great workout!",
            messages.Count
        );
        return messages;
    }
}
