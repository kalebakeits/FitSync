namespace FitSync.Uploader.Features.ActivityProcessing.Services;

using FitSync.Shared.Features.GlobalVariables.DTOs;
using FitSync.Shared.Messages;
using FitSync.Uploader.Features.Kafka.Services;
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
        await foreach (
            ActivityFetchedEvent message in this.kafkaConsumer.ConsumeAsync(cancellationToken)
        )
        {
            using IServiceScope processorScope = this.serviceProvider.CreateScope();
            IActivityProcessor processor =
                processorScope.ServiceProvider.GetRequiredService<IActivityProcessor>();

            this.logger.LogInformation(
                "Kafka message for activity {ActivityId} being consumed. Nom nom",
                message.ActivityId
            );
            await processor.ClaimAndProcessActivityAsync(
                message.ActivityId,
                this.globalVariables.Instance,
                cancellationToken
            );
        }
    }
}
