namespace FitSync.Garmin.Uploader.Features.FitModification.Services;

public interface IFitModifier
{
    byte[] ModifyDeviceInfo(byte[] fitFileData);
}
