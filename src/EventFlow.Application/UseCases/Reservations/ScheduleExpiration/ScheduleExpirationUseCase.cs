using EventFlow.Application.Abstractions;
using EventFlow.Application.Messaging;

namespace EventFlow.Application.UseCases.Reservations.ScheduleExpiration;

public sealed class ScheduleExpirationUseCase(
    IOutboxStore store,
    IClock clock,
    IRequestValidator<ScheduleExpirationRequest> validator)
    : IUseCase<ScheduleExpirationRequest, ScheduleExpirationResponse>, IExpirationScheduler
{
    public async Task<ScheduleExpirationResponse> ExecuteAsync(ScheduleExpirationRequest request, CancellationToken cancellationToken)
    {
        validator.Validate(request);
        var now = clock.UtcNow;
        var ids = await store.ListExpiredReservationIdsAsync(now, request.BatchSize, cancellationToken);
        var messages = ids.Select(id => JobEnvelope.Create(JobTypes.ExpireReservation, id, Guid.NewGuid(), "scheduler", now))
            .ToArray();
        if (messages.Length > 0) await store.EnqueueAsync(messages, cancellationToken);
        return new ScheduleExpirationResponse(messages.Length);
    }

    public async Task<int> EnqueueExpiredAsync(CancellationToken cancellationToken = default) =>
        (await ExecuteAsync(new ScheduleExpirationRequest(), cancellationToken)).Enqueued;
}
