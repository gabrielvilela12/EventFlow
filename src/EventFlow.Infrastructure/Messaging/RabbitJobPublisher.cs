using System.Text.Json;
using EventFlow.Application.Abstractions;
using EventFlow.Application.Messaging;
using RabbitMQ.Client;

namespace EventFlow.Infrastructure.Messaging;

internal sealed class RabbitJobPublisher(RabbitConnection connection, RabbitMqOptions options) : IJobPublisher, IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly SemaphoreSlim gate = new(1, 1);
    private IChannel? channel;

    public Task PublishMainAsync(JobEnvelope message, CancellationToken cancellationToken) =>
        PublishAsync(message, options.MainExchange, cancellationToken);

    public Task PublishRetryAsync(JobEnvelope message, CancellationToken cancellationToken) =>
        PublishAsync(message, options.RetryExchange, cancellationToken);

    private async Task PublishAsync(JobEnvelope message, string exchange, CancellationToken cancellationToken)
    {
        if (!options.Enabled) throw new InvalidOperationException("RabbitMQ transport is disabled.");
        await gate.WaitAsync(cancellationToken);
        try
        {
            var publishChannel = await GetChannelAsync(cancellationToken);
            await RabbitTopology.EnsureAsync(publishChannel, options, cancellationToken);
            var properties = new BasicProperties
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = message.MessageId.ToString("D"),
                CorrelationId = message.CorrelationId.ToString("D"),
                Type = message.JobType,
                Timestamp = new AmqpTimestamp(message.RequestedAtUtc.ToUnixTimeSeconds())
            };
            var body = JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);
            await publishChannel.BasicPublishAsync(exchange, RabbitTopology.QueueFor(message.JobType), true, properties, body, cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (channel?.IsOpen == true) return channel;
        if (channel is not null) await channel.DisposeAsync();
        var rabbitConnection = await connection.GetAsync(cancellationToken);
        channel = await rabbitConnection.CreateChannelAsync(
            new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true),
            cancellationToken);
        return channel;
    }

    public async ValueTask DisposeAsync()
    {
        if (channel is not null) await channel.DisposeAsync();
        gate.Dispose();
    }
}
