namespace EventFlow.Domain;

public sealed class Reservation
{
    private Reservation() { }

    private Reservation(Guid eventId, string customerEmail, int quantity, DateTimeOffset now, TimeSpan lifetime)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        CustomerEmail = customerEmail;
        Quantity = quantity;
        Status = ReservationStatus.Pending;
        ExpiresAtUtc = now.Add(lifetime);
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public string CustomerEmail { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public ReservationStatus Status { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public string? TicketCode { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public int Version { get; private set; }
    public EventDefinition Event { get; private set; } = null!;

    public static Reservation Create(Guid eventId, string customerEmail, int quantity, DateTimeOffset now, TimeSpan lifetime)
    {
        if (eventId == Guid.Empty) 
            throw new ArgumentException("Event is required.", nameof(eventId));

        if (string.IsNullOrWhiteSpace(customerEmail) || !customerEmail.Contains('@'))
            throw new ArgumentException("A valid e-mail is required.", nameof(customerEmail));

        if (quantity is < 1 or > 8) 
            throw new ArgumentOutOfRangeException(nameof(quantity), "Choose between 1 and 8 seats.");

        if (lifetime <= TimeSpan.Zero) 
            throw new ArgumentOutOfRangeException(nameof(lifetime));

        return new Reservation(eventId, customerEmail.Trim().ToLowerInvariant(), quantity, now, lifetime);
    }

    public void Confirm(DateTimeOffset now)
    {
        EnsureStatus(ReservationStatus.Pending);
        if (now >= ExpiresAtUtc) throw new InvalidOperationException("Expired reservations cannot be confirmed.");
        ChangeStatus(ReservationStatus.Confirmed, now);
    }

    public void Reject(DateTimeOffset now)
    {
        EnsureStatus(ReservationStatus.Pending);
        ChangeStatus(ReservationStatus.Rejected, now);
    }

    public void Expire(DateTimeOffset now)
    {
        EnsureStatus(ReservationStatus.Pending);
        if (now < ExpiresAtUtc) throw new InvalidOperationException("Reservation is not expired yet.");
        ChangeStatus(ReservationStatus.Expired, now);
    }

    public void IssueTicket(string ticketCode, DateTimeOffset now)
    {
        EnsureStatus(ReservationStatus.Confirmed);
        if (string.IsNullOrWhiteSpace(ticketCode)) throw new ArgumentException("Ticket code is required.", nameof(ticketCode));
        TicketCode = ticketCode;
        ChangeStatus(ReservationStatus.TicketIssued, now);
    }

    public void MarkNotified(DateTimeOffset now)
    {
        EnsureStatus(ReservationStatus.TicketIssued);
        ChangeStatus(ReservationStatus.Notified, now);
    }

    public void Cancel(DateTimeOffset now)
    {
        if (Status is ReservationStatus.Cancelled or ReservationStatus.Expired or ReservationStatus.Rejected)
            return;
        ChangeStatus(ReservationStatus.Cancelled, now);
    }

    private void EnsureStatus(ReservationStatus expected)
    {
        if (Status != expected) throw new InvalidOperationException($"Expected {expected}, but reservation is {Status}.");
    }

    private void ChangeStatus(ReservationStatus status, DateTimeOffset now)
    {
        Status = status;
        UpdatedAtUtc = now;
        Version++;
    }
}
