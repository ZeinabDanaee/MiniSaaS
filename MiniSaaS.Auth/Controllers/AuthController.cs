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
            Tenant = tenant,
            TenantId = tenant.Id,
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
            return Unauthorized("Invalid credentials");

        var result = _hasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.Password);

        if (result == PasswordVerificationResult.Failed)
            return Unauthorized("Invalid credentials");

        var accessToken = _jwt.CreateToken(user);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = _jwt.GenerateRefreshToken(),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            accessToken,
            refreshToken = refreshToken.Token
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(string refreshToken)
    {
        var token = await _db.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == refreshToken);

        if (token == null)
            return Unauthorized("Invalid refresh token");

        if (token.IsRevoked)
            return Unauthorized("Token revoked");

        if (token.ExpiresAt < DateTime.UtcNow)
            return Unauthorized("Token expired");

        var newAccessToken = _jwt.CreateToken(token.User);

        return Ok(new
        {
            accessToken = newAccessToken
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(string refreshToken)
    {
        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == refreshToken);

        if (token != null)
        {
            token.IsRevoked = true;
            await _db.SaveChangesAsync();
        }

        return Ok();
    }
}