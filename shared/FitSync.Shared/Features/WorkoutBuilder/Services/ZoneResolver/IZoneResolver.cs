namespace FitSync.Shared.Features.WorkoutBuilder.Services.ZoneResolver;

using Dynastream.Fit;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public interface IZoneResolver
{
    (uint Low, uint High)? Resolve(
        WktStepTarget targetType,
        Sport sport,
        int zone,
        ZoneProfile profile
    );
}
