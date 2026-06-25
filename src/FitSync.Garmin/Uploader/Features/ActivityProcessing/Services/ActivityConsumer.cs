namespace FitSync.Garmin.Uploader.Features.ActivityProcessing.Services;

using FitSync.Garmin.Uploader.Features.Kafka.Services;
using FitSync.Shared.Features.GlobalVariables.DTOs;
using Microsoft.Extensions.Logging;

public class ActivityConsumer(
    IServiceProvider serviceProvider,
    IKafkaConsumer kafkaConsumer,
    GlobalVariables globalVariables,
    ILogger<ActivityConsumer> logger
) : IActivityConsumer
{
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly IKafkaConsumer kafkaConsumer = kafkaConsumer;
    private readonly GlobalVariables globalVariables = globalVariables;
    private readonly ILogger<ActivityConsumer> logger = logger;

    public async Task ConsumeActivitiesAsync(CancellationToken cancellationToken)
    {
        await foreach (string message in this.kafkaConsumer.ConsumeAsync(cancellationToken))
        {
            using IServiceScope processorScope = this.serviceProvider.CreateScope();
            IActivityProcessor processor =
                processorScope.ServiceProvider.GetRequiredService<IActivityProcessor>();

            this.logger.LogInformation(
                "Kafka message for activity {ActivityId} being consumed. Nom nom",
                message
            );
            await processor.ClaimAndProcessActivityAsync(
                new Guid(message),
                this.globalVariables.Instance,
                cancellationToken
            );
        }
    }
}
