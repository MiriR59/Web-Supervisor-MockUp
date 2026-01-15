using Microsoft.EntityFrameworkCore;
using WSV.Api.Models;
using WSV.Api.Services;

namespace WSV.Api.Data;

public static class UserSeeder
{
    public static async Task SeedUserAsync(
        AppDbContext db,
        IPasswordService passwordService,
        IConfiguration config,
        string sectionName)
    {
        var section = config.GetSection(sectionName);

        var userName = section["UserName"];
        var password = section["Password"];
        // Default role set as a Viewer, such to be safe
        var role = section["Role"];

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            throw new InvalidOperationException($"{sectionName} configuration missing");

        var exists = await db.AppUsers.AnyAsync(u => u.UserName == userName);
        if (exists)
            return;
        
        var user = new AppUser
        {
            UserName = userName,
            Role = role,
            IsActive = true
        };

        user.PasswordHash = passwordService.Hash(user, password);

        db.AppUsers.Add(user);
        await db.SaveChangesAsync();
    }
}