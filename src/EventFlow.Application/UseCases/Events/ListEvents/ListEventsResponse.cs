using EventFlow.Application.Common;

namespace EventFlow.Application.UseCases.Events.ListEvents;

public sealed record ListEventsResponse(IReadOnlyList<EventView> Events);
