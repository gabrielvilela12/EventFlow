using EventFlow.Application.Abstractions;

namespace EventFlow.Application.UseCases.Reservations.CancelReservation;

public sealed class CancelReservationValidator : IRequestValidator<CancelReservationRequest>
{
    public void Validate(CancelReservationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Id == Guid.Empty) throw new ArgumentException("Reservation id is required.", nameof(request.Id));
    }
}
