using EventFlow.Application.Common;

namespace EventFlow.Application.UseCases.Reservations.ListReservations;

public sealed record ListReservationsResponse(IReadOnlyList<ReservationView> Reservations);
