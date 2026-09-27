namespace EventFlow.Application.UseCases.Reservations.CreateReservation;

public sealed record CreateReservationRequest(Guid EventId, string CustomerEmail, int Quantity);
