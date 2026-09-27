using EventFlow.Domain;
using Xunit;

namespace EventFlow.Tests;

public sealed class ReservationTests
{
    private static readonly DateTimeOffset Now = new(2030, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_starts_pending_with_normalized_email()
    {
        var reservation = Reservation.Create(Guid.NewGuid(), "  DEV@EXAMPLE.COM ", 2, Now, TimeSpan.FromMinutes(15));

        Assert.Equal(ReservationStatus.Pending, reservation.Status);
        Assert.Equal("dev@example.com", reservation.CustomerEmail);
        Assert.Equal(Now.AddMinutes(15), reservation.ExpiresAtUtc);
    }

    [Fact]
    public void Confirm_rejects_expired_reservation()
    {
        var reservation = Reservation.Create(Guid.NewGuid(), "dev@example.com", 1, Now, TimeSpan.FromMinutes(15));

        var error = Assert.Throws<InvalidOperationException>(() => reservation.Confirm(Now.AddMinutes(15)));

        Assert.Contains("Expired", error.Message);
    }

    [Fact]
    public void Ticket_follows_confirmed_reservation()
    {
        var reservation = Reservation.Create(Guid.NewGuid(), "dev@example.com", 1, Now, TimeSpan.FromMinutes(15));
        reservation.Confirm(Now.AddMinutes(1));

        reservation.IssueTicket("EVT-DEMO-001", Now.AddMinutes(2));

        Assert.Equal(ReservationStatus.TicketIssued, reservation.Status);
        Assert.Equal("EVT-DEMO-001", reservation.TicketCode);
    }

    [Fact]
    public void Event_does_not_oversell_seats()
    {
        var eventItem = EventDefinition.Create("Demo", "Lab", DateTimeOffset.UtcNow.AddDays(2), 3);

        Assert.True(eventItem.TryReserve(2));
        Assert.False(eventItem.TryReserve(2));
        Assert.Equal(1, eventItem.AvailableSeats);
    }

    [Fact]
    public void Cancel_is_idempotent()
    {
        var reservation = Reservation.Create(Guid.NewGuid(), "dev@example.com", 1, Now, TimeSpan.FromMinutes(15));

        reservation.Cancel(Now.AddMinutes(1));
        reservation.Cancel(Now.AddMinutes(2));

        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        Assert.Equal(1, reservation.Version);
    }
}
