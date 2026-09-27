using EventFlow.Application.Messaging;

namespace EventFlow.Application.UseCases.Jobs.ProcessReservation;

public sealed record ProcessReservationRequest(JobEnvelope Message);
