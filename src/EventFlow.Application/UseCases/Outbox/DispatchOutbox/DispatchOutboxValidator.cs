using EventFlow.Application.Abstractions;

namespace EventFlow.Application.UseCases.Outbox.DispatchOutbox;

public sealed class DispatchOutboxValidator : IRequestValidator<DispatchOutboxRequest>
{
    public void Validate(DispatchOutboxRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.BatchSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(request.BatchSize));
    }
}
