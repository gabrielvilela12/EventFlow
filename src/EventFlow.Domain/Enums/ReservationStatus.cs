namespace EventFlow.Domain;

public enum ReservationStatus
{
    Pending = 1,
    Confirmed = 2,
    TicketIssued = 3,
    Notified = 4,
    Rejected = 5,
    Expired = 6,
    Cancelled = 7
}
