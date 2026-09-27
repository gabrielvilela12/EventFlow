namespace EventFlow.Application.UseCases.Jobs.ProcessReservation;

public sealed record ProcessReservationResponse(bool AlreadyProcessed, string Detail);
