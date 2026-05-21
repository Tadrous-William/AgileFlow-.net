using AgileTaskManager.Data;
using AgileTaskManager.Models.Entities;
using AgileTaskManager.Models.ViewModels;
using AgileTaskManager.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace AgileTaskManager.Controllers.Api;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProjectsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet("user-projects")]
    public async Task<IActionResult> GetUserProjects()
    {
        var userId = _userManager.GetUserId(User);
        var projects = await _db.ProjectMembers
            .Where(pm => pm.UserId == userId)
            .Select(pm => new
            {
                id = pm.Project.Id,
                name = pm.Project.Name,
                description = pm.Project.Description,
                role = pm.Role.ToString()
            })
            .ToListAsync();

        return Ok(projects);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _db.Projects
            .Include(p => p.Members)
            .Include(p => p.Sprints)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(p => p.Name.Contains(keyword));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProjectListItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                TeamSize = p.Members.Count,
                SprintsCount = p.Sprints.Count
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProjectCreateViewModel vm)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = _userManager.GetUserId(User);

        var project = new Project
        {
            Name = vm.Name?.Trim() ?? throw new InvalidOperationException("Project name is required."),
            Description = vm.Description?.Trim(),
            StartDate = vm.StartDate,
            EndDate = vm.EndDate,
            CreatedAt = DateTime.UtcNow
        };

        // Add user as project member (as TeamLead)
        if (string.IsNullOrEmpty(userId))
            return BadRequest(new { error = "User not authenticated." });
            
        var userExists = await _userManager.FindByIdAsync(userId);
        if (userExists == null)
            return BadRequest(new { error = "User not found." });

        _db.Projects.Add(project);
        project.Members.Add(new ProjectMember
        {
            UserId = userId,
            Role = AgileTaskManager.Models.Enums.ProjectMemberRole.TeamLead,
            JoinedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = project.Id }, new
        {
            id = project.Id,
            name = project.Name
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var project = await _db.Projects
            .Include(p => p.Members)
                .ThenInclude(m => m.User)
            .Include(p => p.Sprints)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
            return NotFound();

        return Ok(project);
    }

    [HttpGet("{id:int}/members")]
    public async Task<IActionResult> GetMembers(int id)
    {
        var members = await _db.ProjectMembers
            .Where(pm => pm.ProjectId == id)
            .Include(pm => pm.User)
            .Select(pm => new
            {
                userId = pm.UserId,
                userName = pm.User.FullName,
                role = pm.Role.ToString()
            })
            .ToListAsync();

        return Ok(members);
    }

    [HttpPost("{id:int}/members")]
    [Authorize]
    public async Task<IActionResult> AddMember(int id, [FromBody] AddProjectMemberViewModel vm)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var currentUserId = _userManager.GetUserId(User);
        var isLead = await _db.ProjectMembers.AnyAsync(pm => pm.ProjectId == id && pm.UserId == currentUserId && pm.Role == AgileTaskManager.Models.Enums.ProjectMemberRole.TeamLead);
        if (!isLead && !User.IsInRole(RoleConstants.Admin))
            return Forbid();

        var projectExists = await _db.Projects.AnyAsync(p => p.Id == id);
        if (!projectExists)
            return NotFound();

        var userExists = await _db.Users.AnyAsync(u => u.Id == vm.UserId);
        if (!userExists)
            return BadRequest(new { error = "User not found." });

        var alreadyMember = await _db.ProjectMembers.AnyAsync(pm => pm.ProjectId == id && pm.UserId == vm.UserId);
        if (alreadyMember)
            return BadRequest(new { error = "User is already a team member of this project." });

        var member = new ProjectMember
        {
            ProjectId = id,
            UserId = vm.UserId,
            Role = vm.Role
        };

        _db.ProjectMembers.Add(member);
        await _db.SaveChangesAsync();
        return Ok(member);
    }

    [HttpDelete("{id:int}/members/{userId}")]
    [Authorize]
    public async Task<IActionResult> RemoveMember(int id, string userId)
    {
        var currentUserId = _userManager.GetUserId(User);
        var isLead = await _db.ProjectMembers.AnyAsync(pm => pm.ProjectId == id && pm.UserId == currentUserId && pm.Role == AgileTaskManager.Models.Enums.ProjectMemberRole.TeamLead);
        if (!isLead && !User.IsInRole(RoleConstants.Admin))
            return Forbid();

        var member = await _db.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == id && pm.UserId == userId);

        if (member == null)
            return NotFound();

        _db.ProjectMembers.Remove(member);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
