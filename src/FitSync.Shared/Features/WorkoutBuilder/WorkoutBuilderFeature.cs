namespace FitSync.Shared.Features.WorkoutBuilder;

using FitSync.Shared.Features.WorkoutBuilder.Services.Messages.WorkoutItem;
using FitSync.Shared.Features.WorkoutBuilder.Services.Messages.Sports.Base;
using FitSync.Shared.Features.WorkoutBuilder.Services.Messages.Swimming;
using FitSync.Shared.Features.WorkoutBuilder.Services.GarminWorkoutBuilder;
using FitSync.Shared.Features.WorkoutBuilder.Services.WahooWorkoutBuilder;
using FitSync.Shared.Features.WorkoutBuilder.Services.SchemaResolver;
using FitSync.Shared.Features.WorkoutBuilder.Services.Writer;
using FitSync.Shared.Features.WorkoutBuilder.Services.ZoneResolver;
using Microsoft.Extensions.DependencyInjection;

public static class WorkoutBuilderFeature
{
    extension (IServiceCollection services)
    {
        public IServiceCollection AddWorkoutBuilderFeature()
        {
            services.AddTransient<IZoneResolver, ZoneResolver>();
            services.AddTransient<IWorkoutSchemaResolver, WorkoutSchemaResolver>();
            services.AddTransient<IWorkoutStepHandler, WorkoutStepHandler>();
            services.AddTransient<IWorkoutSwimStepHandler, WorkoutSwimStepHandler>();
            services.AddTransient<IWorkoutRepeatHandler, WorkoutRepeatHandler>();
            services.AddTransient<IWorkoutItemResolver, WorkoutItemResolver>();
            services.AddTransient<GenericSportMessageBuilder>();
            services.AddTransient<OpenWaterSwimMessageBuilder>();
            services.AddTransient<PoolSwimMessageBuilder>();
            services.AddTransient<IFitFileEncoder, FitFileEncoder>();
            services.AddTransient<IWorkoutWriter, WorkoutWriter>();
            services.AddTransient<IWahooWorkoutBuilder, WahooWorkoutBuilder>();
            services.AddTransient<IGarminWorkoutBuilder, GarminWorkoutBuilder>();
            return services;
        }
    }
}
