using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WSV.Api.Models;
using WSV.Api.Services;
using WSV.Api.Data;

namespace WSV.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(AppDbContext db, IPasswordService passwordService, IJwtTokenService jwtTokenService)
    {
        _db = db;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
    }

    // POST request, sends JSON body with login, verifies login and issues JWT
    [HttpPost("login")]
    public async Task <IActionResult> Login([FromBody] LoginRequestDto request)
    {
        // 1) Validate input shape, if empty than 400
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // 2) Search for user by username in Db and validate him
        var user = await _db.AppUsers.SingleOrDefaultAsync(u => u.UserName == request.UserName);
        if(user is null || !user.IsActive)
            return Unauthorized();

        // 4) Verify password
        var passwordOk = _passwordService.Verify(user, request.Password);
        if(!passwordOk)
            return Unauthorized();
        
        // 5) Issue valid token     
        var token = _jwtTokenService.CreateToken(user);
            return Ok(new LoginTokenDto { Token = token });

    }

}
