using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Scheduler.Controllers;

[ApiController]
[Route("health")]
public sealed class OperationsController : ControllerBase
{
    [HttpGet]
    public IActionResult Health() => Ok(new { status = "healthy", service = "EventFlow.Scheduler", utc = DateTimeOffset.UtcNow });
}
