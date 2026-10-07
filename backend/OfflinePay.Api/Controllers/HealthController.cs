using Microsoft.AspNetCore.Mvc;

namespace OfflinePay.Api.Controllers;

[ApiController]
[Route("api/v1/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "OfflinePay.Api"
        });
    }
}