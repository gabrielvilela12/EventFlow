using EventFlow.Application.Messaging;
using RabbitMQ.Client;

namespace EventFlow.Infrastructure.Messaging;

public static class RabbitTopology
{
    public static string QueueFor(string jobType) => $"eventflow.{ToKebabCase(jobType)}";

    public static async Task EnsureAsync(IChannel channel, RabbitMqOptions options, CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(options.MainExchange, ExchangeType.Direct, true, false, cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(options.RetryExchange, ExchangeType.Direct, true, false, cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(options.DeadExchange, ExchangeType.Direct, true, false, cancellationToken: cancellationToken);

        foreach (var jobType in JobTypes.All)
        {
            var main = QueueFor(jobType);
            var retry = $"{main}.retry";
            var dead = $"{main}.dead";
            var mainArguments = new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = options.DeadExchange,
                ["x-dead-letter-routing-key"] = main
            };
            var retryArguments = new Dictionary<string, object?>
            {
                ["x-message-ttl"] = options.RetryDelayMilliseconds,
                ["x-dead-letter-exchange"] = options.MainExchange,
                ["x-dead-letter-routing-key"] = main
            };

            await channel.QueueDeclareAsync(main, true, false, false, mainArguments, cancellationToken: cancellationToken);
            await channel.QueueDeclareAsync(retry, true, false, false, retryArguments, cancellationToken: cancellationToken);
            await channel.QueueDeclareAsync(dead, true, false, false, cancellationToken: cancellationToken);
            await channel.QueueBindAsync(main, options.MainExchange, main, cancellationToken: cancellationToken);
            await channel.QueueBindAsync(retry, options.RetryExchange, main, cancellationToken: cancellationToken);
            await channel.QueueBindAsync(dead, options.DeadExchange, main, cancellationToken: cancellationToken);
        }
    }

    private static string ToKebabCase(string value) =>
        string.Concat(value.Select((character, index) => char.IsUpper(character) && index > 0
            ? $"-{char.ToLowerInvariant(character)}"
            : char.ToLowerInvariant(character).ToString()));
}
