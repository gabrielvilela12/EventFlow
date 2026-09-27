using EventFlow.Application.Abstractions;

namespace EventFlow.Application.UseCases.Reservations.ScheduleExpiration;

public sealed class ScheduleExpirationValidator : IRequestValidator<ScheduleExpirationRequest>
{
    public void Validate(ScheduleExpirationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.BatchSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(request.BatchSize));
    }
}
