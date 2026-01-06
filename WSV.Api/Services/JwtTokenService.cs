using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WSV.Api.Models;

namespace WSV.Api.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public string CreateToken(AppUser user)
    {
        // Read config
        var jwtSection = _config.GetSection("Jwt");

        var key = jwtSection["Key"]
            ?? throw new InvalidOperationException("Jwt:Key is missing");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var expiresMinutesRaw = jwtSection["ExpiresMinutes"];
        // Safety check for when expiresMinutes might be wrong or missing
        if (!int.TryParse(expiresMinutesRaw, out var expiresMinutes))
            expiresMinutes = 60;

        // Claims (identity + role)
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, user.Role)
        };

        // Create credentials
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        // Create token
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: creds
        );

        // Serialize
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}