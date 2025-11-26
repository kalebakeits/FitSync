namespace FitSync.Uploader.Features.FitModification.Services;

using Dynastream.Fit;

public interface IFitFileDecoder
{
    List<Mesg> DecodeMessages(byte[] fitFileData);
}
