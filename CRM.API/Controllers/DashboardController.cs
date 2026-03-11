using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.API.Data;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    public DashboardController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var srCount = await _db.SalesReps.CountAsync();
        var customerCount = await _db.Customers.CountAsync();

        var leaderboard = await _db.SalesReps
            .Select(sr => new {
                sr.Id,
                sr.Name,
                TasksCompleted = sr.Tasks.Count(t => t.Status == "Completed"),
                TasksPending = sr.Tasks.Count(t => t.Status == "Pending")
            })
            .OrderByDescending(sr => sr.TasksCompleted)
            .Take(10)
            .ToListAsync();

        return Ok(new {
            salesRepCount = srCount,
            customerCount = customerCount,
            leaderboard = leaderboard
        });
    }
}