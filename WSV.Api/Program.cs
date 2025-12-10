using Microsoft.EntityFrameworkCore;
using WSV.Api.Data;
using WSV.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext and tell EF Core to use SQLite with given string in appsettings.json
// 'DefaultConnection' name must match to the one in appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Singleton only creates one instance of cache for whole app
builder.Services.AddSingleton<ISourceBehaviourService, SourceBehaviourService>();
builder.Services.AddSingleton<ILastReadingService, LastReadingService>();
builder.Services.AddHostedService<GeneratorService>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await db.SourceReadings.ExecuteDeleteAsync();

    DbInitializer.Seed(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
