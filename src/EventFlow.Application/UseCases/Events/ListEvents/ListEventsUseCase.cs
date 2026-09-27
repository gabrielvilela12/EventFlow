using EventFlow.Application.Abstractions;
using EventFlow.Application.Common;

namespace EventFlow.Application.UseCases.Events.ListEvents;

public sealed class ListEventsUseCase(IEventFlowRepository repository, IRequestValidator<ListEventsRequest> validator)
    : IUseCase<ListEventsRequest, ListEventsResponse>
{
    public async Task<ListEventsResponse> ExecuteAsync(ListEventsRequest request, CancellationToken cancellationToken)
    {
        validator.Validate(request);
        var events = await repository.ListEventsAsync(cancellationToken);
        return new ListEventsResponse(events.Select(EventView.From).ToArray());
    }
}
