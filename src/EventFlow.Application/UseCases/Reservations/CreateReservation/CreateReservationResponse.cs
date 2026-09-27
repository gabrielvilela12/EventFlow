using EventFlow.Application.Common;

namespace EventFlow.Application.UseCases.Reservations.CreateReservation;

public sealed record CreateReservationResponse(ReservationView Reservation);
