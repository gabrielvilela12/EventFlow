namespace EventFlow.Application.UseCases.Jobs.ExpireReservation;

public sealed record ExpireReservationResponse(bool AlreadyProcessed, string Detail);
