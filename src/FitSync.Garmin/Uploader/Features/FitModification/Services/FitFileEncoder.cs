namespace FitSync.Garmin.Uploader.Features.FitModification.Services;

using Dynastream.Fit;

public class FitFileEncoder : IFitFileEncoder
{
    public byte[] EncodeMessages(List<Mesg> messages)
    {
        using MemoryStream outputStream = new();
        Encode encoder = new(ProtocolVersion.V20);
        encoder.Open(outputStream);

        foreach (Mesg mesg in messages)
        {
            encoder.Write(mesg);
        }

        encoder.Close();
        outputStream.Flush();

        return outputStream.ToArray();
    }
}
