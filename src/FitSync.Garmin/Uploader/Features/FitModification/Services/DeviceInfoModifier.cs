namespace FitSync.Garmin.Uploader.Features.FitModification.Services;

using Dynastream.Fit;

public class DeviceInfoModifier(ILogger<DeviceInfoModifier> logger) : IDeviceInfoModifier
{
    private readonly ILogger<DeviceInfoModifier> logger = logger;
    private const ushort GarminManufacturer = 1;
    private const ushort GarminEdge830Product = 2530;

    public int ModifyDeviceInfo(List<Mesg> messages)
    {
        this.logger.LogDebug("Modifying file data. (Spoofing really)");
        int modificationsCount = 0;

        for (int i = 0; i < messages.Count; i++)
        {
            Mesg mesg = messages[i];

            if (mesg.Num == MesgNum.FileId)
            {
                FileIdMesg fileIdMesg = new(mesg);
                fileIdMesg.SetManufacturer(GarminManufacturer);
                fileIdMesg.SetProduct(GarminEdge830Product);

                messages[i] = fileIdMesg;

                modificationsCount++;
            }
            else if (mesg.Num == MesgNum.DeviceInfo)
            {
                DeviceInfoMesg deviceInfoMesg = new(mesg);
                deviceInfoMesg.SetManufacturer(GarminManufacturer);
                deviceInfoMesg.SetProduct(GarminEdge830Product);

                messages[i] = deviceInfoMesg;

                modificationsCount++;
            }
        }
        this.logger.LogDebug("Sucessfully edited (spoofed) file data");
        return modificationsCount;
    }
}
