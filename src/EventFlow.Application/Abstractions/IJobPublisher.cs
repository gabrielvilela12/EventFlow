using EventFlow.Application.Messaging;

namespace EventFlow.Application.Abstractions;

public interface IJobPublisher
{
    Task PublishMainAsync(JobEnvelope message, CancellationToken cancellationToken);
    Task PublishRetryAsync(JobEnvelope message, CancellationToken cancellationToken);
}
