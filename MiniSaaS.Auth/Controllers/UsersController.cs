using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSaaS.Auth.Data;
using MiniSaaS.Auth.Models;
using MiniSaaS.Auth.Services;
using System.Security.Claims;

namespace MiniSaaS.Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

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

    [Authorize]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Users
            .Select(x => new
            {
                x.Email,
                x.TenantId
            })
            .ToListAsync();

        return Ok(users);
    }
}