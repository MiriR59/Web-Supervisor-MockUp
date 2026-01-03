using Microsoft.AspNetCore.Identity;
using WSV.Api.Models;

namespace WSV.Api.Services;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<AppUser> _hasher = new();

    public string Hash(AppUser user, string password)
        => _hasher.HashPassword(user, password);

    public bool Verify(AppUser user, string password)
        => _hasher.VerifyHashedPassword(user, user.PasswordHash, password)
            == PasswordVerificationResult.Success;
}