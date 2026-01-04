using Microsoft.EntityFrameworkCore;
using WSV.Api.Data;
using WSV.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext and tell EF Core to use SQLite with given string in appsettings.json
// 'DefaultConnection' name must match to the one in appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Singleton only creates one instance of cache for whole app
builder.Services.AddSingleton<ISourceBehaviourService, SourceBehaviourService>();
builder.Services.AddSingleton<ILastReadingService, LastReadingService>();

builder.Services.AddHostedService<GeneratorService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Services necessary for token auth
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSection = builder.Configuration.GetSection("Jwt");
    var key = jwtSection["Key"];
    var issuer = jwtSection["Issuer"];
    var audience = jwtSection["Audience"];

    if (string.IsNullOrWhiteSpace(key))
        throw new InvalidOperationException("Jwt:Key is missing");
    if (string.IsNullOrWhiteSpace(issuer))
        throw new InvalidOperationException("Jwt:Issuer is missing");
    if (string.IsNullOrWhiteSpace(audience))
        throw new InvalidOperationException("Jwt:Audience is missing");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),

        ValidateIssuer = true,
        ValidIssuer = issuer,

        ValidateAudience = true,
        ValidAudience = audience,

        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30) // small tolerance for clock drift
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<AppDbContext>();
    var passwordService = services.GetRequiredService<IPasswordService>();
    var config = services.GetRequiredService<IConfiguration>();
    
    await UserSeeder.SeedAdminAsync(db, passwordService, config);

    await db.SourceReadings.ExecuteDeleteAsync();
    await db.Sources.ExecuteDeleteAsync();

    DbInitializer.Seed(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
