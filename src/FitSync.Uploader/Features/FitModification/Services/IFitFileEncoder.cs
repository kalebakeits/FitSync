namespace FitSync.Uploader.Features.FitModification.Services;

using Dynastream.Fit;

public interface IFitFileEncoder
{
    byte[] EncodeMessages(List<Mesg> messages);
}
