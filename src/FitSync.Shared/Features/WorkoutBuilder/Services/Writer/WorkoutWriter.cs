namespace FitSync.Shared.Features.WorkoutBuilder.Services.Writer;

using Dynastream.Fit;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;
using FitSync.Shared.Features.WorkoutBuilder.Services.Messages;
using FitSync.Shared.Features.WorkoutBuilder.Services.Messages.Sports.Base;
using FitSync.Shared.Features.WorkoutBuilder.Services.Messages.Swimming;

public class WorkoutWriter(
    GenericSportMessageBuilder genericSportMessageBuilder,
    OpenWaterSwimMessageBuilder openWaterSwimMessageBuilder,
    PoolSwimMessageBuilder poolSwimMessageBuilder,
    IFitFileEncoder fitFileEncoder
) : IWorkoutWriter
{
    private readonly GenericSportMessageBuilder genericSportMessageBuilder =
        genericSportMessageBuilder;
    private readonly OpenWaterSwimMessageBuilder openWaterSwimMessageBuilder =
        openWaterSwimMessageBuilder;
    private readonly PoolSwimMessageBuilder poolSwimMessageBuilder = poolSwimMessageBuilder;
    private readonly IFitFileEncoder fitFileEncoder = fitFileEncoder;

    public byte[] BuildWorkout(WorkoutSchema schema)
    {
        return schema.Match(
            @default =>
                @default.Sport == Sport.Swimming
                    ? this.Build(@default, this.openWaterSwimMessageBuilder)
                    : this.Build(@default, this.genericSportMessageBuilder),
            poolSwim => this.Build(poolSwim, this.poolSwimMessageBuilder)
        );
    }

    private byte[] Build<TSchema>(TSchema schema, IMessageBuilder<TSchema> builder)
        where TSchema : WorkoutSchema
    {
        List<WorkoutStepMesg> workoutSteps = builder.BuildWorkoutStepMessages(schema);
        WorkoutMesg workoutMesg = builder.BuildWorkoutMessage(schema, (ushort)workoutSteps.Count);
        return this.fitFileEncoder.Encode(workoutMesg, workoutSteps);
    }
}
