using EventFlow.Application.Messaging;

namespace EventFlow.Application.Abstractions;

public interface IJobHandler
{
    string JobType { get; }
    Task HandleAsync(JobEnvelope message, CancellationToken cancellationToken);
}
