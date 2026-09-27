using EventFlow.Application.Abstractions;
using EventFlow.Application.UseCases.Events.CreateEvent;
using EventFlow.Application.UseCases.Events.ListEvents;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Api.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController(
    IUseCase<CreateEventRequest, CreateEventResponse> createEvent,
    IUseCase<ListEventsRequest, ListEventsResponse> listEvents)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await listEvents.ExecuteAsync(
            new ListEventsRequest(),
            cancellationToken);

        return Ok(result.Events);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateEventRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createEvent.ExecuteAsync(
            request,
            cancellationToken);

        return Created(
            $"/api/events/{result.Event.Id}",
            result.Event);
    }
}