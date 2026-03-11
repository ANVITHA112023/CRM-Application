using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.API.Data;
using CRM.API.Models;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly AppDbContext _db;
    public CustomerController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var customers = await _db.Customers
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        return Ok(customer);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Customer customer)
    {
        if (string.IsNullOrWhiteSpace(customer.Name))
            return BadRequest("Customer name is required.");
        customer.CreatedAt = DateTime.UtcNow;
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return Ok(customer);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] Customer updated)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        customer.Name       = updated.Name;
        customer.Email      = updated.Email;
        customer.Phone      = updated.Phone;
        customer.Company    = updated.Company;
        customer.Address    = updated.Address;
        customer.Status     = updated.Status;
        customer.SalesRepId = updated.SalesRepId; // ← assign SR

        await _db.SaveChangesAsync();
        return Ok(customer);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Deleted" });
    }
}