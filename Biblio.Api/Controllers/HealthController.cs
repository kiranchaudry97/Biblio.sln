using Microsoft.AspNetCore.Mvc;

namespace Biblio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
 [HttpGet]
 public IActionResult Get() => Ok(new { status = "ok" });
}