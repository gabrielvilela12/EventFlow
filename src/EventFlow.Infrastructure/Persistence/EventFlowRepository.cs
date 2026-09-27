using System.Text.Json;
using EventFlow.Application.Abstractions;
using EventFlow.Application.Messaging;
using EventFlow.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Infrastructure.Persistence;

internal sealed class EventFlowRepository(EventFlowDbContext db) : IEventFlowRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task AddEventAsync(EventDefinition item, CancellationToken cancellationToken)
    {
        db.Events.Add(item);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EventDefinition>> ListEventsAsync(CancellationToken cancellationToken) =>
        await db.Events.AsNoTracking().OrderBy(x => x.StartsAtUtc).ToListAsync(cancellationToken);

    public Task<EventDefinition?> FindEventAsync(Guid id, CancellationToken cancellationToken) =>
        db.Events.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddReservationWithOutboxAsync(
        Reservation reservation, JobEnvelope message, CancellationToken cancellationToken)
    {
        var outbox = OutboxMessage.Create(message.MessageId, message.CorrelationId, message.JobType, 
            reservation.Id, JsonSerializer.Serialize(message, JsonOptions), message.RequestedAtUtc);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        db.Reservations.Add(reservation);
        db.OutboxMessages.Add(outbox);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public Task<Reservation?> FindReservationAsync(Guid id, CancellationToken cancellationToken) =>
        db.Reservations.Include(x => x.Event).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Reservation>> ListReservationsAsync(CancellationToken cancellationToken) =>
        await db.Reservations.AsNoTracking().Include(x => x.Event)
            .OrderByDescending(x => x.CreatedAtUtc).ToListAsync(cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _ = await db.SaveChangesAsync(cancellationToken);
}
