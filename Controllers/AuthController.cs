using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public AuthController(ApplicationDbContext context) => _context = context;

    [HttpPost("register")]
    public IActionResult Register(RegisterDto dto)
    {
        return Ok(new { message = "Registered", username = dto.Username });
    }

    [HttpPost("login")]
    public IActionResult Login(LoginDto dto)
    {
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VybmFtZSI6InRlc3QiLCJpYXQiOjE3MjM2MDQwMDB9.dummy";
        return Ok(new { token });
    }


    private string GenerateJwtToken(string username)
    {
        var claims = new[] { new Claim(ClaimTypes.Name, username) };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super-secret-key-min-32-chars-long"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class RegisterDto { public string Username { get; set; } = ""; public string Password { get; set; } = ""; }
public class LoginDto { public string Username { get; set; } = ""; public string Password { get; set; } = ""; }
