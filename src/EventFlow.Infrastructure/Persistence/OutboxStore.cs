using System.Text.Json;
using EventFlow.Application.Abstractions;
using EventFlow.Application.Messaging;
using EventFlow.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Infrastructure.Persistence;

internal sealed class OutboxStore(EventFlowDbContext db) : IOutboxStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<OutboxMessage>> ListUnpublishedAsync(int limit, CancellationToken cancellationToken) =>
        await db.OutboxMessages.Where(x => x.PublishedAtUtc == null)
            .OrderBy(x => x.CreatedAtUtc).Take(limit).ToListAsync(cancellationToken);

    public async Task MarkPublishedAsync(OutboxMessage message, DateTimeOffset now, CancellationToken cancellationToken)
    {
        message.MarkPublished(now);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> ListExpiredReservationIdsAsync(
        DateTimeOffset now, int limit, CancellationToken cancellationToken) =>
        await db.Reservations
            .Where(x => x.Status == ReservationStatus.Pending && x.ExpiresAtUtc <= now)
            .Where(x => !db.OutboxMessages.Any(m => m.ReservationId == x.Id && m.JobType == JobTypes.ExpireReservation))
            .Select(x => x.Id).Take(limit).ToListAsync(cancellationToken);

    public async Task EnqueueAsync(IReadOnlyCollection<JobEnvelope> messages, CancellationToken cancellationToken)
    {
        db.OutboxMessages.AddRange(messages.Select(message => OutboxMessage.Create(message.MessageId,
            message.CorrelationId, message.JobType, message.ReservationId,
            JsonSerializer.Serialize(message, JsonOptions), message.RequestedAtUtc)));
        await db.SaveChangesAsync(cancellationToken);
    }
}
