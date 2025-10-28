using Biblio.Models.Seed;
using Microsoft.AspNetCore.Mvc;

namespace Biblio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
 private readonly IServiceProvider _sp;
 public SeedController(IServiceProvider sp) => _sp = sp;

 [HttpPost]
 public async Task<IActionResult> Post()
 {
 await SeedData.InitializeAsync(_sp);
 return Ok();
 }
}