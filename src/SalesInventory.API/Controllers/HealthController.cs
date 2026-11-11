using Microsoft.AspNetCore.Mvc;

namespace SalesInventory.Api.Controllers;

/// <summary>
/// Liveness probe: reports that the API process is up and serving requests.
/// Deliberately does not touch the database, so it stays green even when SQL Server is down.
/// </summary>
[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    // Must stay [AllowAnonymous] once authentication is added (Week 4),
    // so load balancers / container health checks can call it without a token.
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok" });
}
