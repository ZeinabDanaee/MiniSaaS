using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MiniSaaS.Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        var email =
            User.FindFirstValue(ClaimTypes.Email);

        var role =
            User.FindFirstValue(ClaimTypes.Role);

        var tenantId =
            User.FindFirst("tenant_id")?.Value;

        return Ok(new
        {
            UserId = userId,
            Email = email,
            Role = role,
            TenantId = tenantId
        });
    }
}