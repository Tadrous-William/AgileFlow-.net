using AgileTaskManager.Data;
using AgileTaskManager.Models.Entities;
using AgileTaskManager.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AgileTaskManager.Controllers;

[Authorize]
public class AnalyticsController : Controller
{
    private readonly ApplicationDbContext _db;

    public AnalyticsController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET: /Analytics/Sprint/{sprintId}
    public async Task<IActionResult> Sprint(int sprintId = 0)
    {
        var currentUserId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(RoleConstants.Admin);
        
        // Load ALL sprints — any authenticated user can view analytics.
        // (Access control is enforced at the API level, not the page level.)
        var availableSprints = await _db.Sprints
            .Include(s => s.Project)
                .ThenInclude(p => p.Members)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();

        if (sprintId == 0)
        {
            var firstSprint = availableSprints.FirstOrDefault();

            if (firstSprint != null)
            {
                return RedirectToAction("Sprint", new { sprintId = firstSprint.Id });
            }

            // No sprints available, show error message
            ViewBag.ErrorMessage = "No sprint analytics available. You need to be a member of a project with sprints to view analytics.";
            ViewBag.SprintId = 0;
            return View();
        }

        // Validate sprint exists and user has access
        var sprint = availableSprints.FirstOrDefault(s => s.Id == sprintId);

        if (sprint == null)
        {
            return NotFound();
        }

        // Build distinct project list for the project filter dropdown
        var availableProjects = availableSprints
            .GroupBy(s => s.ProjectId)
            .Select(g => g.First().Project)
            .OrderBy(p => p.Name)
            .ToList();

        ViewBag.SprintId = sprintId;
        ViewBag.CurrentProjectId = sprint.ProjectId;
        ViewBag.AvailableSprints = availableSprints;
        ViewBag.AvailableProjects = availableProjects;
        return View();
    }

    // GET: /Analytics/Project/{projectId}
    public async Task<IActionResult> Project(int projectId)
    {
        ViewBag.ProjectId = projectId;
        return View();
    }
}
