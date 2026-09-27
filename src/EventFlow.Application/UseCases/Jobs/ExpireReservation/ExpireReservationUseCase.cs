using EventFlow.Application.Abstractions;
using EventFlow.Application.Messaging;
using EventFlow.Domain;

namespace EventFlow.Application.UseCases.Jobs.ExpireReservation;

public sealed class ExpireReservationUseCase(
    IJobProcessingStore store,
    IClock clock,
    IRequestValidator<ExpireReservationRequest> validator)
    : IUseCase<ExpireReservationRequest, ExpireReservationResponse>, IJobHandler
{
    public string JobType => JobTypes.ExpireReservation;

    public async Task<ExpireReservationResponse> ExecuteAsync(ExpireReservationRequest request, CancellationToken cancellationToken)
    {
        validator.Validate(request);
        var message = request.Message;
        if (await store.WasCompletedAsync(message.MessageId, cancellationToken))
            return new ExpireReservationResponse(true, "Message already processed.");

        var reservation = await store.FindReservationAsync(message.ReservationId, includeEvent: false, cancellationToken)
            ?? throw new KeyNotFoundException($"Reservation {message.ReservationId} was not found.");
        var now = clock.UtcNow;
        string detail;
        if (reservation.Status == ReservationStatus.Pending && now >= reservation.ExpiresAtUtc)
        {
            reservation.Expire(now);
            detail = "Pending reservation expired.";
        }
        else
            detail = $"Ignored because status is {reservation.Status} or the deadline has not elapsed.";

        await store.CompleteAsync(message, detail, null, cancellationToken);
        return new ExpireReservationResponse(false, detail);
    }

    public async Task HandleAsync(JobEnvelope message, CancellationToken cancellationToken) =>
        _ = await ExecuteAsync(new ExpireReservationRequest(message), cancellationToken);
}
