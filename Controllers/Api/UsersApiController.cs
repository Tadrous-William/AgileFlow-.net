using AgileTaskManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgileTaskManager.Controllers.Api;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public UsersApiController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _db.Users
            .OrderBy(u => u.FullName)
            .Select(u => new
            {
                id = u.Id,
                fullName = u.FullName,
                email = u.Email,
                role = u.Role
            })
            .ToListAsync();

        return Ok(users);
    }
}
