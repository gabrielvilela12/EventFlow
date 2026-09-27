namespace EventFlow.Application.UseCases.Jobs.GenerateTicket;

public sealed record GenerateTicketResponse(bool AlreadyProcessed, string Detail);
