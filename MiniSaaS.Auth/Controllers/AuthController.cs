using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSaaS.Auth.Data;
using MiniSaaS.Auth.DTOs;
using MiniSaaS.Auth.Models;
using MiniSaaS.Auth.Services;

namespace MiniSaaS.Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _hasher;
    private readonly JwtService _jwt;

    public AuthController(
        AppDbContext db,
        IPasswordHasher<User> hasher,
        JwtService jwt)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var exists = await _db.Users
            .AnyAsync(x => x.Email == dto.Email);

        if (exists)
            return BadRequest("Email already exists");

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = dto.CompanyName
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            FullName = dto.FullName,
            Role = "Admin",
            Tenant = tenant
        };

        user.PasswordHash =
            _hasher.HashPassword(user, dto.Password);

        _db.Users.Add(user);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            Message = "Tenant and admin user created successfully",
            TenantId = tenant.Id,
            UserId = user.Id
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _db.Users
            .Include(x => x.Tenant)
            .FirstOrDefaultAsync(x => x.Email == dto.Email);

        if (user == null)
            return Unauthorized("Invalid email or password");

        var result = _hasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.Password);

        if (result == PasswordVerificationResult.Failed)
            return Unauthorized("Invalid email or password");

        var accessToken = _jwt.CreateToken(user);

        return Ok(new
        {
            accessToken
        });
    }
}