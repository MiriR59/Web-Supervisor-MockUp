using Microsoft.EntityFrameworkCore;
using WSV.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext and tell EF Core to use SQLite with given string in appsettings.json
// 'DefaultConnection' name must match to the one in appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
