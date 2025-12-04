using Microsoft.EntityFrameworkCore;
using WSV.Api.Models;

namespace WSV.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Gotta add DbSet Machine and MachineReading later
    public DbSet<Source> Sources { get; set; }
    public DbSet<SourceReading> SourceReadings { get; set; }
}