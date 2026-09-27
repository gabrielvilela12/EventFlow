namespace EventFlow.Domain;

public sealed class OutboxMessage
{
    private OutboxMessage() { }

    private OutboxMessage(Guid messageId, Guid correlationId, string jobType, Guid reservationId, string payload, DateTimeOffset createdAtUtc)
    {
        Id = messageId;
        CorrelationId = correlationId;
        JobType = jobType;
        ReservationId = reservationId;
        Payload = payload;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid CorrelationId { get; private set; }
    public string JobType { get; private set; } = string.Empty;
    public Guid ReservationId { get; private set; }
    public string Payload { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? PublishedAtUtc { get; private set; }

    public static OutboxMessage Create(Guid messageId, Guid correlationId, string jobType, Guid reservationId, string payload, DateTimeOffset now) =>
        new(messageId, correlationId, jobType, reservationId, payload, now);

    public void MarkPublished(DateTimeOffset now) => PublishedAtUtc = now;
}
