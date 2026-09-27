using EventFlow.Application.Abstractions;
using EventFlow.Application.Common;

namespace EventFlow.Application.UseCases.Reservations.GetReservation;

public sealed class GetReservationUseCase(IEventFlowRepository repository, IRequestValidator<GetReservationRequest> validator)
    : IUseCase<GetReservationRequest, GetReservationResponse>
{
    public async Task<GetReservationResponse> ExecuteAsync(GetReservationRequest request, CancellationToken cancellationToken)
    {
        validator.Validate(request);
        var reservation = await repository.FindReservationAsync(request.Id, cancellationToken);
        return new GetReservationResponse(reservation is null ? null : ReservationView.From(reservation, reservation.Event.Name));
    }
}
