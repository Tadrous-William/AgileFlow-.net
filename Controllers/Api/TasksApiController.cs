using AgileTaskManager.Data;
using AgileTaskManager.Models.ViewModels;
using AgileTaskManager.Services.Interfaces;
using AgileTaskManager.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgileTaskManager.Models.Entities;

namespace AgileTaskManager.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _tasks;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public TasksController(ITaskService tasks, UserManager<ApplicationUser> userManager, ApplicationDbContext db)
    { _tasks = tasks; _userManager = userManager; _db = db; }

    // GET api/tasks
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = _userManager.GetUserId(User);
        var role = User.IsInRole(RoleConstants.Admin) || User.IsInRole(RoleConstants.TeamLead) ? "Admin" : "Developer";
        return Ok(await _tasks.GetAllAsync(userId, role));
    }

    // GET api/tasks/by-project/5
    [HttpGet("by-project/{projectId:int}")]
    public async Task<IActionResult> GetByProject(int projectId)
    {
        var tasks = await _db.Tasks
            .Where(t => t.ProjectId == projectId)
            .OrderBy(t => t.Title)
            .Select(t => new { id = t.Id, title = t.Title, status = t.Status.ToString() })
            .ToListAsync();
        return Ok(tasks);
    }

    // GET api/tasks/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _tasks.GetDetailsAsync(id);
        return task == null ? NotFound() : Ok(task);
    }

    // POST api/tasks
    [HttpPost]
    [Authorize(Roles = "Admin,TeamLead,Developer,Member")]
    public async Task<IActionResult> Create([FromBody] TaskCreateViewModel vm)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (vm.StartDate.HasValue && vm.Deadline.HasValue && vm.StartDate.Value > vm.Deadline.Value)
            return BadRequest(new { error = "Start date must not be after end date." });
        var creatorId = _userManager.GetUserId(User)!;
        var task = await _tasks.CreateAsync(vm, creatorId);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    // PUT api/tasks/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,TeamLead,Developer,Member")]
    public async Task<IActionResult> Update(int id, [FromBody] TaskEditViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (vm.StartDate.HasValue && vm.Deadline.HasValue && vm.StartDate.Value > vm.Deadline.Value)
            return BadRequest(new { error = "Start date must not be after end date." });
        var actorId = _userManager.GetUserId(User)!;
        await _tasks.UpdateAsync(vm, actorId);
        return NoContent();
    }

    // DELETE api/tasks/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,TeamLead")]
    public async Task<IActionResult> Delete(int id)
    {
        await _tasks.DeleteAsync(id);
        return NoContent();
    }

    // PATCH api/tasks/5/status
    [HttpPatch("{id}/status"), Authorize(Roles = "Admin,TeamLead,Developer,Member")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] AgileTaskManager.Models.Enums.TaskStatus newStatus)
    {
        var actorId = _userManager.GetUserId(User)!;
        try
        {
            await _tasks.UpdateStatusAsync(id, newStatus, actorId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
