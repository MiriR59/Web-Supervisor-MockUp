using WSV.Api.Models;
using WSV.Api.Services;

namespace WSV.Api.Data;

public static class UserSeeder
{
    public static async Task SeedAdminAsync(
        AppDbContext db,
        IPasswordService passwordService,
        IConfiguration config)
    {
        if (db.AppUsers.Any())
            return;

        // Access appsettings.dev.json for AdminSeed details
        var section = config.GetSection("AdminSeed");

        var userName = section["UserName"];
        var password = section["Password"];
        var role = section["Role"] ?? "Admin";

        // Check if config settings exists
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("AdminSeed configuration missing");

        var admin = new AppUser
        {
            UserName = userName,
            Role = role,
            IsActive = true
        };

        admin.PasswordHash = passwordService.Hash(admin, password);

        db.AppUsers.Add(admin);
        await db.SaveChangesAsync();
    }
}