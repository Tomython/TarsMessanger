using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterDto dto)
    {
        return Ok(new { message = "Registered", username = dto.Username });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        return Ok(new { token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.dummy-token-for-mvp" });
    }
}

public class RegisterDto { public string Username { get; set; } = ""; public string Password { get; set; } = ""; }
public class LoginDto { public string Username { get; set; } = ""; public string Password { get; set; } = ""; }
