using AgileTaskManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgileTaskManager.Controllers;

[Authorize(Roles = "Admin,TeamLead")]
public class KanbanController : Controller
{
    private readonly ApplicationDbContext _db;

    public KanbanController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET: /Kanban
    public async Task<IActionResult> Index(int? projectId = null, int? sprintId = null)
    {
        ViewBag.ProjectId = projectId;
        ViewBag.Projects = await _db.Projects.OrderBy(p => p.Name).ToListAsync();

        if (projectId.HasValue)
        {
            var sprints = await _db.Sprints
                .Where(s => s.ProjectId == projectId)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();
            ViewBag.Sprints = sprints;

            // Auto-select sprint: use provided sprintId, else pick the active sprint,
            // else the most recent planned sprint, else the first available sprint.
            if (!sprintId.HasValue)
            {
                var activeSprint = sprints.FirstOrDefault(s => s.Status == AgileTaskManager.Models.Enums.SprintStatus.Active)
                    ?? sprints.FirstOrDefault(s => s.Status == AgileTaskManager.Models.Enums.SprintStatus.Planned)
                    ?? sprints.FirstOrDefault();
                sprintId = activeSprint?.Id;
            }
        }

        ViewBag.SprintId = sprintId;
        return View();
    }
}
