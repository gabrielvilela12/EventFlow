using EventFlow.Application.Abstractions;
using EventFlow.Application.Messaging;
using EventFlow.Domain;

namespace EventFlow.Application.UseCases.Jobs.ProcessReservation;

public sealed class ProcessReservationUseCase(
    IJobProcessingStore store,
    IClock clock,
    IRequestValidator<ProcessReservationRequest> validator)
    : IUseCase<ProcessReservationRequest, ProcessReservationResponse>, IJobHandler
{
    public string JobType => JobTypes.ProcessReservation;

    public async Task<ProcessReservationResponse> ExecuteAsync(ProcessReservationRequest request, CancellationToken cancellationToken)
    {
        validator.Validate(request);
        var message = request.Message;
        if (await store.WasCompletedAsync(message.MessageId, cancellationToken))
            return new ProcessReservationResponse(true, "Message already processed.");

        var reservation = await store.FindReservationAsync(message.ReservationId, includeEvent: true, cancellationToken)
            ?? throw new KeyNotFoundException($"Reservation {message.ReservationId} was not found.");
        var now = clock.UtcNow;
        JobEnvelope? next = null;
        string detail;

        if (reservation.Status != ReservationStatus.Pending)
            detail = $"Ignored because status is {reservation.Status}.";
        else if (now >= reservation.ExpiresAtUtc)
        {
            reservation.Expire(now);
            detail = "Reservation expired before processing.";
        }
        else if (!reservation.Event.TryReserve(reservation.Quantity))
        {
            reservation.Reject(now);
            detail = "Rejected because the event has insufficient seats.";
        }
        else
        {
            reservation.Confirm(now);
            next = JobEnvelope.Create(JobTypes.GenerateTicket, reservation.Id, message.CorrelationId, "worker", now);
            detail = "Seats reserved and reservation confirmed.";
        }

        await store.CompleteAsync(message, detail, next, cancellationToken);
        return new ProcessReservationResponse(false, detail);
    }

    public async Task HandleAsync(JobEnvelope message, CancellationToken cancellationToken) =>
        _ = await ExecuteAsync(new ProcessReservationRequest(message), cancellationToken);
}
