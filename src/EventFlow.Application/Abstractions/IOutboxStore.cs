using EventFlow.Application.Messaging;
using EventFlow.Domain;

namespace EventFlow.Application.Abstractions;

public interface IOutboxStore
{
    Task<IReadOnlyList<OutboxMessage>> ListUnpublishedAsync(int limit, CancellationToken cancellationToken);
    Task MarkPublishedAsync(OutboxMessage message, DateTimeOffset now, CancellationToken cancellationToken);
    Task<IReadOnlyList<Guid>> ListExpiredReservationIdsAsync(DateTimeOffset now, int limit, CancellationToken cancellationToken);
    Task EnqueueAsync(IReadOnlyCollection<JobEnvelope> messages, CancellationToken cancellationToken);
}
