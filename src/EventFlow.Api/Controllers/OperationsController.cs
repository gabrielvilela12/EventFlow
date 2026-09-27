using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Api.Controllers;

[ApiController]
[Route("health")]
public sealed class OperationsController : ControllerBase
{
    [HttpGet(Name = "Health")]
    public IActionResult Health() => Ok(new { status = "healthy", service = "EventFlow.Api", utc = DateTimeOffset.UtcNow });
}
