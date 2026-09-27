using System.Text.Json;
using EventFlow.Application.Abstractions;
using EventFlow.Application.Messaging;
using EventFlow.Infrastructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventFlow.Worker;

public sealed class QueueWorker(
    RabbitConnection connection,
    RabbitMqOptions options,
    IJobPublisher publisher,
    IServiceScopeFactory scopeFactory,
    ILogger<QueueWorker> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Enabled)
        {
            logger.LogWarning("RabbitMQ consumer is disabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var rabbitConnection = await connection.GetAsync(stoppingToken);
                await using var channel = await rabbitConnection.CreateChannelAsync(cancellationToken: stoppingToken);
                await RabbitTopology.EnsureAsync(channel, options, stoppingToken);
                await channel.BasicQosAsync(0, options.PrefetchCount, false, stoppingToken);

                foreach (var jobType in JobTypes.All)
                {
                    var consumer = new AsyncEventingBasicConsumer(channel);
                    consumer.ReceivedAsync += (_, delivery) => ProcessAsync(channel, delivery, stoppingToken);
                    await channel.BasicConsumeAsync(RabbitTopology.QueueFor(jobType), false, consumer, stoppingToken);
                }

                logger.LogInformation("Worker subscribed to {Count} queues with prefetch {Prefetch}.",
                    JobTypes.All.Count, options.PrefetchCount);
                while (channel.IsOpen && !stoppingToken.IsCancellationRequested)
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception error)
            {
                logger.LogError(error, "Consumer unavailable; retrying in 5 seconds.");
                await DelayReconnectAsync(stoppingToken);
            }
        }
    }

    private async Task ProcessAsync(IChannel channel, BasicDeliverEventArgs delivery, CancellationToken cancellationToken)
    {
        JobEnvelope? message;
        try
        {
            message = JsonSerializer.Deserialize<JobEnvelope>(delivery.Body.Span, JsonOptions);
            JobEnvelopeValidation.Validate(message);
        }
        catch (Exception error) when (error is JsonException or InvalidOperationException)
        {
            logger.LogError(error, "Invalid message {DeliveryTag}; routing to dead-letter queue.", delivery.DeliveryTag);
            await channel.BasicNackAsync(delivery.DeliveryTag, false, false, cancellationToken);
            return;
        }

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetRequiredService<IEnumerable<IJobHandler>>()
                .SingleOrDefault(x => x.JobType == message!.JobType);
            if (handler is null)
            {
                await channel.BasicNackAsync(delivery.DeliveryTag, false, false, cancellationToken);
                return;
            }

            await handler.HandleAsync(message!, cancellationToken);
            await channel.BasicAckAsync(delivery.DeliveryTag, false, cancellationToken);
            logger.LogInformation("Completed {JobType} message {MessageId}.", message!.JobType, message.MessageId);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            if (channel.IsOpen) await channel.BasicNackAsync(delivery.DeliveryTag, false, true, CancellationToken.None);
        }
        catch (Exception error)
        {
            logger.LogError(error, "Failed {JobType} message {MessageId} at attempt {Attempt}.",
                message!.JobType, message.MessageId, message.Attempt + 1);
            if (message.Attempt + 1 < options.MaxAttempts)
            {
                try
                {
                    await publisher.PublishRetryAsync(message with { Attempt = message.Attempt + 1 }, cancellationToken);
                    await channel.BasicAckAsync(delivery.DeliveryTag, false, cancellationToken);
                }
                catch
                {
                    await channel.BasicNackAsync(delivery.DeliveryTag, false, true, cancellationToken);
                    throw;
                }
            }
            else
            {
                await channel.BasicNackAsync(delivery.DeliveryTag, false, false, cancellationToken);
            }
        }
    }

    private static async Task DelayReconnectAsync(CancellationToken cancellationToken)
    {
        try { await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken); }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
    }
}
