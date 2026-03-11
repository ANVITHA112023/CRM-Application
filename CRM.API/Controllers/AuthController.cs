using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.API.Data;
using CRM.API.Models;
using CRM.API.Services;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;

    public AuthController(AppDbContext db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("Email already registered.");

        var user = new User
        {
            Name     = dto.Name,
            Email    = dto.Email,
            Role     = "SalesRep",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Auto-create SalesRep record linked to this user
        var sr = new SalesRep
        {
            Name        = dto.Name,
            Email       = dto.Email,
            Phone       = dto.Phone ?? "",
            City        = dto.City ?? "",
            Expertise   = dto.Expertise ?? "",
            IsAvailable = true
        };
        _db.SalesReps.Add(sr);
        await _db.SaveChangesAsync();

        // Link user to salesrep
        user.SalesRepId = sr.Id;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Registered successfully", salesRepId = sr.Id });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid email or password.");

        // Find linked SalesRep if SR role
        int? salesRepId = null;
        if (user.Role == "SalesRep")
        {
            var sr = await _db.SalesReps.FirstOrDefaultAsync(s => s.Email == user.Email);
            salesRepId = sr?.Id ?? user.SalesRepId;
        }

        var token = _jwt.GenerateToken(user);

        return Ok(new
        {
            token,
            role       = user.Role,
            userName   = user.Name,
            userId     = user.Id,
            salesRepId = salesRepId
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout() => Ok(new { message = "Logged out" });
}

public class RegisterDto
{
    public string Name      { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
    public string Password  { get; set; } = string.Empty;
    public string? Phone    { get; set; }
    public string? City     { get; set; }
    public string? Expertise { get; set; }
}

public class LoginDto
{
    public string Email    { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}