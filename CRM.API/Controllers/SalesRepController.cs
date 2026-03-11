using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.API.Data;
using CRM.API.Models;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesRepController : ControllerBase
{
    private readonly AppDbContext _db;
    public SalesRepController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var reps = await _db.SalesReps
            .Select(s => new {
                s.Id, s.Name, s.Email, s.Phone, s.City,
                s.Expertise, s.IsAvailable, s.ProfilePicture,
                TaskCount = _db.Tasks.Count(t => t.SalesRepId == s.Id && t.Status == "Pending")
            })
            .OrderByDescending(s => s.IsAvailable)
            .ThenBy(s => s.Name)
            .ToListAsync();
        return Ok(reps);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var sr = await _db.SalesReps
            .Where(s => s.Id == id)
            .Select(s => new {
                s.Id, s.Name, s.Email, s.Phone, s.City,
                s.Expertise, s.IsAvailable, s.ProfilePicture,
                Tasks = _db.Tasks
                    .Where(t => t.SalesRepId == s.Id)
                    .Select(t => new {
                        t.Id, t.Title, t.Description,
                        t.Status, t.DueDate, t.CompletedAt, t.SalesRepId
                    })
                    .OrderByDescending(t => t.DueDate)
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (sr == null) return NotFound();
        return Ok(sr);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] SalesRep sr)
    {
        _db.SalesReps.Add(sr);
        await _db.SaveChangesAsync();
        return Ok(sr);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SalesRep updated)
    {
        var sr = await _db.SalesReps.FindAsync(id);
        if (sr == null) return NotFound();
        sr.Name        = updated.Name;
        sr.Email       = updated.Email;
        sr.Phone       = updated.Phone;
        sr.City        = updated.City;
        sr.Expertise   = updated.Expertise;
        sr.IsAvailable = updated.IsAvailable;
        await _db.SaveChangesAsync();

        // Also update the linked User name/email
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == sr.Email || u.SalesRepId == id);
        if (user != null)
        {
            user.Name  = updated.Name;
            user.Email = updated.Email;
            await _db.SaveChangesAsync();
        }

        return Ok(sr);
    }

    // ── Password reset endpoint ──────────────────────────────
    [HttpPut("{id}/password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
            return BadRequest("Password must be at least 6 characters.");

        var sr = await _db.SalesReps.FindAsync(id);
        if (sr == null) return NotFound();

        // Find linked user by email or SalesRepId
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == sr.Email || u.SalesRepId == id);
        if (user == null) return NotFound("No user account found for this sales rep.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Password updated successfully." });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var sr = await _db.SalesReps.FindAsync(id);
        if (sr == null) return NotFound();
        _db.SalesReps.Remove(sr);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Deleted" });
    }
}

public class ResetPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}