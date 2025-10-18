using Microsoft.AspNetCore.Mvc;

namespace InvoicesService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            service = "InvoicesService",
            timestamp = DateTime.UtcNow
        });
    }
}
