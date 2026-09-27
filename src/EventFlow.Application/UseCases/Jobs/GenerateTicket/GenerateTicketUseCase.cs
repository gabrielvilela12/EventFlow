using EventFlow.Application.Abstractions;
using EventFlow.Application.Messaging;
using EventFlow.Domain;

namespace EventFlow.Application.UseCases.Jobs.GenerateTicket;

public sealed class GenerateTicketUseCase(
    IJobProcessingStore store,
    IClock clock,
    IRequestValidator<GenerateTicketRequest> validator)
    : IUseCase<GenerateTicketRequest, GenerateTicketResponse>, IJobHandler
{
    public string JobType => JobTypes.GenerateTicket;

    public async Task<GenerateTicketResponse> ExecuteAsync(GenerateTicketRequest request, CancellationToken cancellationToken)
    {
        validator.Validate(request);
        var message = request.Message;
        if (await store.WasCompletedAsync(message.MessageId, cancellationToken))
            return new GenerateTicketResponse(true, "Message already processed.");

        var reservation = await store.FindReservationAsync(message.ReservationId, includeEvent: false, cancellationToken)
            ?? throw new KeyNotFoundException($"Reservation {message.ReservationId} was not found.");
        JobEnvelope? next = null;
        string detail;
        if (reservation.Status == ReservationStatus.Confirmed)
        {
            var ticketCode = $"EVT-{reservation.Id:N}"[..16].ToUpperInvariant();
            var now = clock.UtcNow;
            reservation.IssueTicket(ticketCode, now);
            next = JobEnvelope.Create(JobTypes.SendNotification, reservation.Id, message.CorrelationId, "worker", now);
            detail = $"Ticket {ticketCode} generated.";
        }
        else
            detail = $"Ignored because status is {reservation.Status}.";

        await store.CompleteAsync(message, detail, next, cancellationToken);
        return new GenerateTicketResponse(false, detail);
    }

    public async Task HandleAsync(JobEnvelope message, CancellationToken cancellationToken) =>
        _ = await ExecuteAsync(new GenerateTicketRequest(message), cancellationToken);
}
