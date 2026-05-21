using AgileTaskManager.Data;
using AgileTaskManager.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AgileTaskManager.Controllers;

[Authorize(Roles = "Admin,TeamLead")]
public class SprintController : Controller
{
    private readonly ApplicationDbContext _db;

    public SprintController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET: /Sprint  — list all sprints the user can see, grouped by project
    public async Task<IActionResult> Index(int? projectId = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(RoleConstants.Admin);

        // Projects the user is a member of (or all for admin)
        var projectsQuery = _db.Projects
            .Include(p => p.Members)
            .Include(p => p.Sprints)
            .AsQueryable();

        var projects = (await projectsQuery.OrderBy(p => p.Name).ToListAsync())
            .Where(p => isAdmin || p.Members.Any(m => m.UserId == userId))
            .ToList();

        ViewBag.Projects = projects;
        ViewBag.SelectedProjectId = projectId;

        if (projectId.HasValue)
        {
            var sprints = await _db.Sprints
                .Where(s => s.ProjectId == projectId.Value)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();
            ViewBag.Sprints = sprints;
            ViewBag.SelectedProject = projects.FirstOrDefault(p => p.Id == projectId.Value);
        }

        return View();
    }
}
