using EventFlow.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Scheduler.Controllers;

[ApiController]
[Route("jobs")]
public sealed class JobsController(IOutboxDispatcher dispatcher, IExpirationScheduler scheduler) : ControllerBase
{
    [HttpPost("dispatch")]
    public async Task<IActionResult> Dispatch(CancellationToken cancellationToken) =>
        Ok(new { published = await dispatcher.DispatchAsync(cancellationToken) });

    [HttpPost("expire")]
    public async Task<IActionResult> Expire(CancellationToken cancellationToken) =>
        Ok(new { enqueued = await scheduler.EnqueueExpiredAsync(cancellationToken) });
}
