namespace FitSync.Garmin.Uploader.Features.FitModification.Services;

using Dynastream.Fit;

public interface IDeviceInfoModifier
{
    int ModifyDeviceInfo(List<Mesg> messages);
}
