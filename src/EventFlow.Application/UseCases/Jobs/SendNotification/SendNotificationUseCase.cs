using EventFlow.Application.Abstractions;
using EventFlow.Application.Messaging;
using EventFlow.Domain;

namespace EventFlow.Application.UseCases.Jobs.SendNotification;

public sealed class SendNotificationUseCase(
    IJobProcessingStore store,
    INotificationSender sender,
    IClock clock,
    IRequestValidator<SendNotificationRequest> validator)
    : IUseCase<SendNotificationRequest, SendNotificationResponse>, IJobHandler
{
    public string JobType => JobTypes.SendNotification;

    public async Task<SendNotificationResponse> ExecuteAsync(SendNotificationRequest request, CancellationToken cancellationToken)
    {
        validator.Validate(request);
        var message = request.Message;
        if (await store.WasCompletedAsync(message.MessageId, cancellationToken))
            return new SendNotificationResponse(true, "Message already processed.");

        var reservation = await store.FindReservationAsync(message.ReservationId, includeEvent: false, cancellationToken)
            ?? throw new KeyNotFoundException($"Reservation {message.ReservationId} was not found.");
        string detail;
        if (reservation.Status == ReservationStatus.TicketIssued)
        {
            await sender.SendDemoAsync(reservation, cancellationToken);
            reservation.MarkNotified(clock.UtcNow);
            detail = "Simulated notification sent.";
        }
        else
            detail = $"Ignored because status is {reservation.Status}.";

        await store.CompleteAsync(message, detail, null, cancellationToken);
        return new SendNotificationResponse(false, detail);
    }

    public async Task HandleAsync(JobEnvelope message, CancellationToken cancellationToken) =>
        _ = await ExecuteAsync(new SendNotificationRequest(message), cancellationToken);
}
