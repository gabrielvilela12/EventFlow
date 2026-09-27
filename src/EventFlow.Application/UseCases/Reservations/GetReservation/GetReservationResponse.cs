using EventFlow.Application.Common;

namespace EventFlow.Application.UseCases.Reservations.GetReservation;

public sealed record GetReservationResponse(ReservationView? Reservation);
