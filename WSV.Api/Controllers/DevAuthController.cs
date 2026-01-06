using Microsoft.AspNetCore.Mvc;
using WSV.Api.Models;
using WSV.Api.Services;

namespace WSV.Api.Controllers;

// Tests whether hashing works as intended
[ApiController]
[Route("api/[controller]")]
public class DevAuthController : ControllerBase
{
    private readonly IPasswordService _passwordService;
    private readonly IWebHostEnvironment _env;

    public DevAuthController(IPasswordService passwordService, IWebHostEnvironment env)
    {
        _passwordService = passwordService;
        _env = env;
    }

    [HttpGet("hashtest")]
    public IActionResult CheckHash()
    {
        if (!_env.IsDevelopment())
            return NotFound();

        var user = new AppUser { UserName = "tester"};
        string password = "right";

        string hash1 = _passwordService.Hash(user, password);
        string hash2 = _passwordService.Hash(user, password);

        bool ok = _passwordService.Verify(new AppUser { PasswordHash = hash1 }, password);

        bool wrong = _passwordService.Verify(new AppUser { PasswordHash = hash1 }, "wrong");

        return Ok(new
        {
            hashedAreDifferent = hash1 != hash2,
            verifyCorrectPassword = ok,
            verifyWrongPasword = wrong
        });
    }
}