using Microsoft.EntityFrameworkCore;
using CRM.API.Models;

namespace CRM.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<SalesRep> SalesReps => Set<SalesRep>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            Name = "Admin",
            Email = "admin@crm.com",
            PasswordHash = "$2a$11$nVfjdp8FlfNBJNucBL5fDOcTiX/C5XrMS0/1b4QBshguShVh4qneu",
            Role = "Admin",
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.SalesRep)
            .WithMany(sr => sr.Tasks)
            .HasForeignKey(t => t.SalesRepId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}