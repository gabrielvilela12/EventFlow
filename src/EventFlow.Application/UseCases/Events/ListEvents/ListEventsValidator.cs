using EventFlow.Application.Abstractions;

namespace EventFlow.Application.UseCases.Events.ListEvents;

public sealed class ListEventsValidator : IRequestValidator<ListEventsRequest>
{
    public void Validate(ListEventsRequest request) => ArgumentNullException.ThrowIfNull(request);
}
