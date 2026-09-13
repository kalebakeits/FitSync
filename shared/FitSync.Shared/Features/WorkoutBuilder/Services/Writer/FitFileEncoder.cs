namespace FitSync.Shared.Features.WorkoutBuilder.Services.Writer;

using Dynastream.Fit;

public class FitFileEncoder : IFitFileEncoder
{
    public byte[] Encode(WorkoutMesg workoutMesg, List<WorkoutStepMesg> workoutSteps)
    {
        FileIdMesg fileIdMesg = new();
        fileIdMesg.SetType(Dynastream.Fit.File.Workout);
        fileIdMesg.SetManufacturer(Manufacturer.Garmin);
        fileIdMesg.SetProduct(0);
        fileIdMesg.SetTimeCreated(new Dynastream.Fit.DateTime(System.DateTime.UtcNow));
        fileIdMesg.SetSerialNumber((uint)Random.Shared.Next());

        MemoryStream stream = new();
        Encode encoder = new(ProtocolVersion.V10);
        encoder.Open(stream);
        encoder.Write(fileIdMesg);
        encoder.Write(workoutMesg);

        foreach (WorkoutStepMesg workoutStep in workoutSteps)
            encoder.Write(workoutStep);

        encoder.Close();
        return stream.ToArray();
    }
}
