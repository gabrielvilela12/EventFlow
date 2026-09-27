using EventFlow.Application.Abstractions;
using EventFlow.Application.Common;

namespace EventFlow.Application.UseCases.Reservations.ListReservations;

public sealed class ListReservationsUseCase(IEventFlowRepository repository, IRequestValidator<ListReservationsRequest> validator)
    : IUseCase<ListReservationsRequest, ListReservationsResponse>
{
    public async Task<ListReservationsResponse> ExecuteAsync(ListReservationsRequest request, CancellationToken cancellationToken)
    {
        validator.Validate(request);
        var reservations = await repository.ListReservationsAsync(cancellationToken);
        return new ListReservationsResponse(reservations.Select(x => ReservationView.From(x, x.Event.Name)).ToArray());
    }
}
