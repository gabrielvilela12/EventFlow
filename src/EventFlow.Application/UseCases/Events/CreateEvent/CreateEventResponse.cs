using EventFlow.Application.Common;

namespace EventFlow.Application.UseCases.Events.CreateEvent;

public sealed record CreateEventResponse(EventView Event);
