using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TarsMessanger.Data;

namespace TarsMessanger.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly MessengerDbContext _context;

    public UsersController(MessengerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .Select(u => new
            {
                id = u.Id,
                username = u.Username,
                email = u.Email
            })
            .ToListAsync();

        return Ok(users);
    }
}
