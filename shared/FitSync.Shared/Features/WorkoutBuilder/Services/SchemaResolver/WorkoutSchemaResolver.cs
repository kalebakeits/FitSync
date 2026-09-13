namespace FitSync.Shared.Features.WorkoutBuilder.Services.SchemaResolver;

using Dynastream.Fit;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;
using FitSync.Shared.Features.WorkoutBuilder.Services.ZoneResolver;

public class WorkoutSchemaResolver(IZoneResolver zoneResolver) : IWorkoutSchemaResolver
{
    private readonly IZoneResolver zoneResolver = zoneResolver;

    public WorkoutSchema Resolve(WorkoutSchema schema, ZoneProfile profile)
    {
        return schema.Match<WorkoutSchema>(
            @default => this.ResolveDefault(@default, profile),
            poolSwim => this.ResolvePoolSwim(poolSwim, profile)
        );
    }

    private WorkoutSchema.Default ResolveDefault(WorkoutSchema.Default schema, ZoneProfile profile)
    {
        return schema with { Items = this.ResolveItems(schema.Items, schema.Sport, profile) };
    }

    private WorkoutSchema.PoolSwim ResolvePoolSwim(
        WorkoutSchema.PoolSwim schema,
        ZoneProfile profile
    )
    {
        return schema with { Items = this.ResolveItems(schema.Items, schema.Sport, profile) };
    }

    private WorkoutItem[] ResolveItems(WorkoutItem[] items, Sport sport, ZoneProfile profile)
    {
        return items.Select(item => this.ResolveItem(item, sport, profile)).ToArray();
    }

    private WorkoutItem ResolveItem(WorkoutItem item, Sport sport, ZoneProfile profile)
    {
        return item.Match<WorkoutItem>(
            step => this.ResolveStep(step, sport, profile),
            swimStep => swimStep,
            repeat => this.ResolveRepeat(repeat, sport, profile)
        );
    }

    private WorkoutItem ResolveStep(WorkoutItem.Step step, Sport sport, ZoneProfile profile)
    {
        if (!step.TargetZone.HasValue)
            return step;

        (uint low, uint high)? resolved = this.zoneResolver.Resolve(
            step.TargetType,
            sport,
            step.TargetZone.Value,
            profile
        );

        if (!resolved.HasValue)
            return step;

        return step with
        {
            TargetLow = resolved.Value.low,
            TargetHigh = resolved.Value.high,
            TargetZone = null,
        };
    }

    private WorkoutItem ResolveRepeat(WorkoutItem.Repeat repeat, Sport sport, ZoneProfile profile)
    {
        return repeat with { Steps = this.ResolveItems(repeat.Steps, sport, profile) };
    }
}
