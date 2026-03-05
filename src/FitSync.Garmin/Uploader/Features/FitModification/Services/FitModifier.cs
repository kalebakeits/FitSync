namespace FitSync.Garmin.Uploader.Features.FitModification.Services;

using Dynastream.Fit;

public class FitModifier(
    IFitFileDecoder decoder,
    IDeviceInfoModifier deviceInfoModifier,
    IFitFileEncoder encoder,
    ILogger<FitModifier> logger
) : IFitModifier
{
    private readonly IFitFileDecoder decoder = decoder;
    private readonly IDeviceInfoModifier deviceInfoModifier = deviceInfoModifier;
    private readonly IFitFileEncoder encoder = encoder;
    private readonly ILogger<FitModifier> logger = logger;

    public byte[] ModifyDeviceInfo(byte[] fitFileData)
    {
        try
        {
            this.logger.LogDebug(
                "Modifying FIT file device info - {Bytes} bytes. Garmin is gonna love this one",
                fitFileData.Length
            );

            List<Mesg> messages = this.decoder.DecodeMessages(fitFileData);

            int modificationsCount = this.deviceInfoModifier.ModifyDeviceInfo(messages);

            byte[] modifiedData = this.encoder.EncodeMessages(messages);

            this.logger.LogInformation(
                "Modified FIT file: {Modifications} messages changed, output: {Bytes} bytes. You are now a Garmin",
                modificationsCount,
                modifiedData.Length
            );

            return modifiedData;
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Failed to modify FIT file, returning unchanged. I tried my best"
            );
            return fitFileData;
        }
    }
}
