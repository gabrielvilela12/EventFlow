using EventFlow.Application.Messaging;

namespace EventFlow.Application.UseCases.Jobs.GenerateTicket;

public sealed record GenerateTicketRequest(JobEnvelope Message);
