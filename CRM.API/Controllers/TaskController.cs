using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.API.Data;
using CRM.API.Models;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly AppDbContext _db;
    public TaskController(AppDbContext db) { _db = db; }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var tasks = await _db.Tasks
            .AsNoTracking()
            .OrderByDescending(t => t.DueDate)
            .Select(t => new {
                t.Id, t.Title, t.Description, t.Status,
                t.DueDate, t.CompletedAt, t.SalesRepId, t.CustomerId
            })
            .ToListAsync();
        return Ok(tasks);
    }

    [HttpGet("salesrep/{srId}")]
    public async Task<IActionResult> GetBySalesRep(int srId)
    {
        var tasks = await _db.Tasks
            .AsNoTracking()
            .Where(t => t.SalesRepId == srId)
            .OrderByDescending(t => t.DueDate)
            .Select(t => new {
                t.Id, t.Title, t.Description, t.Status,
                t.DueDate, t.CompletedAt, t.SalesRepId, t.CustomerId
            })
            .ToListAsync();
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var t = await _db.Tasks.AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new {
                t.Id, t.Title, t.Description, t.Status,
                t.DueDate, t.CompletedAt, t.SalesRepId, t.CustomerId
            })
            .FirstOrDefaultAsync();
        if (t == null) return NotFound();
        return Ok(t);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] TaskItem task)
    {
        task.DueDate = DateTime.SpecifyKind(
            task.DueDate == default ? DateTime.UtcNow : task.DueDate,
            DateTimeKind.Utc
        );
        task.CompletedAt = null;
        task.Status = "Pending";
        task.SalesRep = null; // prevent navigation property issues

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        if (task.CustomerId.HasValue)
        {
            var customer = await _db.Customers.FindAsync(task.CustomerId.Value);
            if (customer != null)
            {
                customer.SalesRepId = task.SalesRepId;
                await _db.SaveChangesAsync();
            }
        }

        return Ok(new {
            task.Id, task.Title, task.Description, task.Status,
            task.DueDate, task.CompletedAt, task.SalesRepId, task.CustomerId
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTaskDto dto)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            task.Status = dto.Status;
            if (dto.Status == "Completed")
                task.CompletedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return Ok(new {
            task.Id, task.Title, task.Status,
            task.DueDate, task.CompletedAt, task.SalesRepId, task.CustomerId
        });
    }

    [HttpPut("{id}/complete")]
    public async Task<IActionResult> Complete(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        task.Status = "Completed";
        task.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new {
            task.Id, task.Title, task.Status,
            task.DueDate, task.CompletedAt, task.SalesRepId, task.CustomerId
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Task deleted" });
    }
}

public class UpdateTaskDto
{
    public string? Status { get; set; }
}