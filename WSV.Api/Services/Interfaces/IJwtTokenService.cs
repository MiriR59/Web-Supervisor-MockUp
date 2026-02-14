using WSV.Api.Models;

namespace WSV.Api.Services;

public interface IJwtTokenService
{
    string CreateToken(AppUser user);
}