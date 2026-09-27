using EventFlow.Application.Messaging;
using EventFlow.Domain;

namespace EventFlow.Application.Abstractions;

public interface IEventFlowRepository
{
    Task AddEventAsync(EventDefinition item, CancellationToken cancellationToken);
    Task<IReadOnlyList<EventDefinition>> ListEventsAsync(CancellationToken cancellationToken);
    Task<EventDefinition?> FindEventAsync(Guid id, CancellationToken cancellationToken);
    Task AddReservationWithOutboxAsync(Reservation reservation, JobEnvelope message, CancellationToken cancellationToken);
    Task<Reservation?> FindReservationAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Reservation>> ListReservationsAsync(CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
