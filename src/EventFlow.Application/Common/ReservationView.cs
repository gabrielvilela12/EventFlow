using EventFlow.Domain;

namespace EventFlow.Application.Common;

public sealed record ReservationView(
    Guid Id,
    Guid EventId,
    string EventName,
    string CustomerEmail,
    int Quantity,
    ReservationStatus Status,
    DateTimeOffset ExpiresAtUtc,
    string? TicketCode,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc)
{
    public static ReservationView From(Reservation item, string eventName) =>
        new(item.Id, item.EventId, eventName, item.CustomerEmail, item.Quantity, item.Status,
            item.ExpiresAtUtc, item.TicketCode, item.CreatedAtUtc, item.UpdatedAtUtc);
}
