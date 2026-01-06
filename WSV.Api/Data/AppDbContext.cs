using Microsoft.EntityFrameworkCore;
using WSV.Api.Models;

namespace WSV.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Source> Sources { get; set; }
    public DbSet<SourceReading> SourceReadings { get; set; }
    public DbSet<AppUser> AppUsers { get; set; }

    // Safety check, no duplicate usernames
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>()
        .HasIndex(u => u.UserName)
        .IsUnique();
    }
}

