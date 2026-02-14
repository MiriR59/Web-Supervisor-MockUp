using WSV.Api.Models;

namespace WSV.Api.Services;

public interface IPasswordService
{
    string Hash(AppUser user, string password);
    bool Verify(AppUser user, string password);
}


