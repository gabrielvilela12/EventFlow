using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace EventFlow.Infrastructure.Messaging;

public sealed class RabbitConnection(RabbitMqOptions options, ILogger<RabbitConnection> logger) : IAsyncDisposable
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private IConnection? connection;

    public async Task<IConnection> GetAsync(CancellationToken cancellationToken)
    {
        if (connection?.IsOpen == true) return connection;
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (connection?.IsOpen == true) return connection;
            if (connection is not null) await connection.DisposeAsync();
            var factory = new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                VirtualHost = options.VirtualHost,
                UserName = options.UserName,
                Password = options.Password,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                RequestedHeartbeat = TimeSpan.FromSeconds(30),
                ConsumerDispatchConcurrency = 1
            };
            connection = await factory.CreateConnectionAsync("eventflow", cancellationToken);
            logger.LogInformation("Connected to RabbitMQ at {Host}:{Port}.", options.HostName, options.Port);
            return connection;
        }
        finally
        {
            gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (connection is not null) await connection.DisposeAsync();
        gate.Dispose();
    }
}
