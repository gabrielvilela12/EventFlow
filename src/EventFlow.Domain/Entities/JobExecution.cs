namespace EventFlow.Domain;

public sealed class JobExecution
{
    private JobExecution() { }

    public JobExecution(Guid messageId, string jobType, Guid reservationId, int attempt, string outcome, string? detail, DateTimeOffset occurredAtUtc)
    {
        Id = Guid.NewGuid();
        MessageId = messageId;
        JobType = jobType;
        ReservationId = reservationId;
        Attempt = attempt;
        Outcome = outcome;
        Detail = detail;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid MessageId { get; private set; }
    public string JobType { get; private set; } = string.Empty;
    public Guid ReservationId { get; private set; }
    public int Attempt { get; private set; }
    public string Outcome { get; private set; } = string.Empty;
    public string? Detail { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
}
