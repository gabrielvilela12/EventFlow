using EventFlow.Application.Abstractions;
using EventFlow.Application.Common;
using EventFlow.Domain;

namespace EventFlow.Application.UseCases.Events.CreateEvent;

public sealed class CreateEventUseCase(IEventFlowRepository repository, IRequestValidator<CreateEventRequest> validator)
    : IUseCase<CreateEventRequest, CreateEventResponse>
{
    public async Task<CreateEventResponse> ExecuteAsync(CreateEventRequest request, CancellationToken cancellationToken)
    {
        validator.Validate(request);
        var item = EventDefinition.Create(request.Name, request.Venue, request.StartsAtUtc, request.Capacity);
        await repository.AddEventAsync(item, cancellationToken);
        return new CreateEventResponse(EventView.From(item));
    }
}
