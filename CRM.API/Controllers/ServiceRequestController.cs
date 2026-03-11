using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.API.Data;
using CRM.API.Models;
using CRM.API.Services;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceRequestController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ExcelService _excel;
    public ServiceRequestController(AppDbContext db, ExcelService excel)
    {
        _db    = db;
        _excel = excel;
    }

    // Public — called from homepage (no auth needed)
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] ServiceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Name and email are required.");

        request.Status    = "Ongoing";
        request.CreatedAt = DateTime.UtcNow;

        // ── Auto-create or update Customer record ──────────────
        var existing = await _db.Customers
            .FirstOrDefaultAsync(c => c.Email == request.Email);

        if (existing == null)
        {
            // New customer — add them
            var customer = new Customer
            {
                Name      = request.Name,
                Email     = request.Email,
                Status    = "Active",
                CreatedAt = DateTime.UtcNow
            };
            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();
            request.CustomerId = customer.Id;
        }
        else
        {
            // Existing customer — just link them
            request.CustomerId = existing.Id;
        }
        // ────────────────────────────────────────────────────────

        _db.ServiceRequests.Add(request);
        await _db.SaveChangesAsync();
        return Ok(request);
    }

    // Admin — get all service requests
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.ServiceRequests
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new {
                r.Id, r.Name, r.Email, r.Message,
                r.Status, r.CreatedAt, r.CustomerId
            })
            .ToListAsync();
        return Ok(list);
    }

    // Admin — get single service request
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var sr = await _db.ServiceRequests.FindAsync(id);
        if (sr == null) return NotFound();
        return Ok(sr);
    }

    // Admin — update status
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] ServiceRequest updated)
    {
        var sr = await _db.ServiceRequests.FindAsync(id);
        if (sr == null) return NotFound();
        sr.Status  = updated.Status;
        sr.Message = updated.Message;
        await _db.SaveChangesAsync();
        return Ok(sr);
    }

    // Admin — export to Excel
    [HttpGet("export")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Export()
    {
        var bytes = await _excel.ExportServiceRequestsAsync();
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "crm-service-requests.xlsx");
    }
}