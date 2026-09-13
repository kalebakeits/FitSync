namespace FitSync.Garmin.Features.FitModification.Services;

public interface IFitModifier
{
    byte[] ModifyDeviceInfo(byte[] fitFileData);
}
