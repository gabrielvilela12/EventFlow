using EventFlow.Application.Abstractions;

namespace EventFlow.Application.UseCases.Reservations.ListReservations;

public sealed class ListReservationsValidator : IRequestValidator<ListReservationsRequest>
{
    public void Validate(ListReservationsRequest request) => ArgumentNullException.ThrowIfNull(request);
}
