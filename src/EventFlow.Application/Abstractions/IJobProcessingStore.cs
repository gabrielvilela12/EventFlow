using EventFlow.Application.Messaging;
using EventFlow.Domain;

namespace EventFlow.Application.Abstractions;

public interface IJobProcessingStore
{
    Task<bool> WasCompletedAsync(Guid messageId, CancellationToken cancellationToken);
    Task<Reservation?> FindReservationAsync(Guid id, bool includeEvent, CancellationToken cancellationToken);
    Task CompleteAsync(JobEnvelope message, string detail, JobEnvelope? nextMessage, CancellationToken cancellationToken);
}
