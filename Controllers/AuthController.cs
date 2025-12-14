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
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (_context.Users.Any(u => u.Username == dto.Username))
            return BadRequest("Username exists");

        var user = new User { Username = dto.Username, PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password) };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Registered" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized();

        var token = GenerateJwtToken(user.Username);
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
