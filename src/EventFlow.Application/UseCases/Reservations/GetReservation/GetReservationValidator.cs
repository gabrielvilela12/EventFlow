using EventFlow.Application.Abstractions;

namespace EventFlow.Application.UseCases.Reservations.GetReservation;

public sealed class GetReservationValidator : IRequestValidator<GetReservationRequest>
{
    public void Validate(GetReservationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Id == Guid.Empty) throw new ArgumentException("Reservation id is required.", nameof(request.Id));
    }
}
