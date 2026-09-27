using System.Text.Json;
using EventFlow.Application.Abstractions;
using EventFlow.Application.Messaging;
using EventFlow.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Infrastructure.Persistence;

internal sealed class JobProcessingStore(EventFlowDbContext db, IClock clock) : IJobProcessingStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task<bool> WasCompletedAsync(Guid messageId, CancellationToken cancellationToken) =>
        db.JobExecutions.AnyAsync(x => x.MessageId == messageId && x.Outcome == "Completed", cancellationToken);

    public Task<Reservation?> FindReservationAsync(Guid id, bool includeEvent, CancellationToken cancellationToken)
    {
        IQueryable<Reservation> query = db.Reservations;
        if (includeEvent) query = query.Include(x => x.Event);
        return query.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task CompleteAsync(
        JobEnvelope message, string detail, JobEnvelope? nextMessage, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        db.JobExecutions.Add(new JobExecution(message.MessageId, message.JobType, message.ReservationId,
            message.Attempt, "Completed", detail, clock.UtcNow));
        if (nextMessage is not null)
            db.OutboxMessages.Add(OutboxMessage.Create(nextMessage.MessageId, nextMessage.CorrelationId,
                nextMessage.JobType, nextMessage.ReservationId,
                JsonSerializer.Serialize(nextMessage, JsonOptions), nextMessage.RequestedAtUtc));
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
