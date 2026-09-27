using EventFlow.Application.Abstractions;

namespace EventFlow.Application.UseCases.Events.CreateEvent;

public sealed class CreateEventValidator(IClock clock) : IRequestValidator<CreateEventRequest>
{
    public void Validate(CreateEventRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Name)) throw new ArgumentException("Event name is required.", nameof(request.Name));
        if (string.IsNullOrWhiteSpace(request.Venue)) throw new ArgumentException("Venue is required.", nameof(request.Venue));
        if (request.Capacity <= 0) throw new ArgumentOutOfRangeException(nameof(request.Capacity), "Capacity must be positive.");
        if (request.StartsAtUtc <= clock.UtcNow) throw new ArgumentException("The event must start in the future.", nameof(request.StartsAtUtc));
    }
}
