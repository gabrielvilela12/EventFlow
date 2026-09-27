using EventFlow.Application.Abstractions;
using EventFlow.Application.Common;
using EventFlow.Application.Messaging;
using EventFlow.Domain;

namespace EventFlow.Application.UseCases.Reservations.CreateReservation;

public sealed class CreateReservationUseCase(
    IEventFlowRepository repository,
    IClock clock,
    IRequestValidator<CreateReservationRequest> validator)
    : IUseCase<CreateReservationRequest, CreateReservationResponse>
{
    public async Task<CreateReservationResponse> ExecuteAsync(CreateReservationRequest request, CancellationToken cancellationToken)
    {
        validator.Validate(request);

        var eventItem = await repository.FindEventAsync(request.EventId, cancellationToken)
            ?? throw new KeyNotFoundException("Event was not found.");
        if (eventItem.StartsAtUtc <= clock.UtcNow) throw new InvalidOperationException("Event has already started.");

        var now = clock.UtcNow;
        var reservation = Reservation.Create(request.EventId, request.CustomerEmail, request.Quantity, now, TimeSpan.FromMinutes(15));
        var message = JobEnvelope.Create(JobTypes.ProcessReservation, reservation.Id, Guid.NewGuid(), "api", now);
        await repository.AddReservationWithOutboxAsync(reservation, message, cancellationToken);
        return new CreateReservationResponse(ReservationView.From(reservation, eventItem.Name));
    }
}
