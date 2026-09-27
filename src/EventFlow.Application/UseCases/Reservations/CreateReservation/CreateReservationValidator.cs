using EventFlow.Application.Abstractions;

namespace EventFlow.Application.UseCases.Reservations.CreateReservation;

public sealed class CreateReservationValidator : IRequestValidator<CreateReservationRequest>
{
    public void Validate(CreateReservationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.EventId == Guid.Empty) throw new ArgumentException("Event is required.", nameof(request.EventId));
        if (string.IsNullOrWhiteSpace(request.CustomerEmail) || !request.CustomerEmail.Contains('@'))
            throw new ArgumentException("A valid e-mail is required.", nameof(request.CustomerEmail));
        if (request.Quantity is < 1 or > 8)
            throw new ArgumentOutOfRangeException(nameof(request.Quantity), "Choose between 1 and 8 seats.");
    }
}
