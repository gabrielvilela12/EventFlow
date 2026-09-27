using EventFlow.Application.Messaging;

namespace EventFlow.Application.UseCases.Jobs.ExpireReservation;

public sealed record ExpireReservationRequest(JobEnvelope Message);
