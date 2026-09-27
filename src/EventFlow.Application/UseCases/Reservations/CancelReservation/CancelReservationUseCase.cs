using EventFlow.Application.Abstractions;
using EventFlow.Domain;

namespace EventFlow.Application.UseCases.Reservations.CancelReservation;

public sealed class CancelReservationUseCase(
    IEventFlowRepository repository,
    IClock clock,
    IRequestValidator<CancelReservationRequest> validator)
    : IUseCase<CancelReservationRequest, CancelReservationResponse>
{
    public async Task<CancelReservationResponse> ExecuteAsync(CancelReservationRequest request, CancellationToken cancellationToken)
    {
        validator.Validate(request);
        var reservation = await repository.FindReservationAsync(request.Id, cancellationToken);
        if (reservation is null) return new CancelReservationResponse(false);
        var release = reservation.Status is ReservationStatus.Confirmed or ReservationStatus.TicketIssued or ReservationStatus.Notified;
        reservation.Cancel(clock.UtcNow);
        if (release) reservation.Event.Release(reservation.Quantity);
        await repository.SaveChangesAsync(cancellationToken);
        return new CancelReservationResponse(true);
    }
}
