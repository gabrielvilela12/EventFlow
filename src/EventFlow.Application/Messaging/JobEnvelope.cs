namespace EventFlow.Application.Messaging;

public sealed record JobEnvelope(
    int SchemaVersion,
    Guid MessageId,
    Guid CorrelationId,
    string JobType,
    Guid ReservationId,
    DateTimeOffset RequestedAtUtc,
    string Source,
    int Attempt)
{
    public const int CurrentSchemaVersion = 1;

    public static JobEnvelope Create(string jobType, Guid reservationId, Guid correlationId, string source, DateTimeOffset now) =>
        new(CurrentSchemaVersion, Guid.NewGuid(), correlationId, jobType, reservationId, now, source, 0);
}
