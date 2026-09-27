namespace EventFlow.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public bool Enabled { get; init; } = true;
    public string HostName { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string VirtualHost { get; init; } = "/";
    public string UserName { get; init; } = "eventflow";
    public string Password { get; init; } = "eventflow-local";
    public string MainExchange { get; init; } = "eventflow.jobs";
    public string RetryExchange { get; init; } = "eventflow.jobs.retry";
    public string DeadExchange { get; init; } = "eventflow.jobs.dead";
    public int RetryDelayMilliseconds { get; init; } = 10_000;
    public int MaxAttempts { get; init; } = 3;
    public ushort PrefetchCount { get; init; } = 4;
}
