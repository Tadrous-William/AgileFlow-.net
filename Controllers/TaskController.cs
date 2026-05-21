using AgileTaskManager.Models.Enums;
using AgileTaskManager.Models.ViewModels;
using AgileTaskManager.Services.Interfaces;
using AgileTaskManager.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AgileTaskManager.Models.Entities;
using AgileTaskManager.Data;
using Microsoft.EntityFrameworkCore;


namespace AgileTaskManager.Controllers;

[Authorize]
public class TaskController : Controller
{
    private readonly ITaskService _tasks;
    private readonly IUserService _users;
    private readonly ICommentService _comments;
    private readonly IExportService _export;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public TaskController(
        ITaskService tasks,
        IUserService users,
        ICommentService comments,
        IExportService export,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        IWebHostEnvironment env)
    {
        _tasks       = tasks;
        _users       = users;
        _comments    = comments;
        _export      = export;
        _userManager = userManager;
        _db          = db;
        _env         = env;
    }

    // GET /Task
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var role = User.IsInRole(RoleConstants.Admin) || User.IsInRole(RoleConstants.TeamLead)
            ? "Admin"
            : "Developer";
        var list   = await _tasks.GetAllAsync(userId, role);
        return View(list);
    }

    // GET /Task/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var task = await _tasks.GetDetailsAsync(id);
        if (task == null) return NotFound();
        return View(task);
    }

    // GET /Task/Create
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        await LoadSelectListsAsync();
        return View(new TaskCreateViewModel());
    }

    // POST /Task/Create
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(TaskCreateViewModel vm)
    {
        if (vm.StartDate.HasValue && vm.Deadline.HasValue && vm.StartDate.Value > vm.Deadline.Value)
            ModelState.AddModelError(nameof(vm.Deadline), "Deadline must not be before start date.");

        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return View(vm);
        }

        var creatorId = _userManager.GetUserId(User)!;
        await _tasks.CreateAsync(vm, creatorId);
        TempData["Success"] = "Task created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Task/Edit/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var task = await _tasks.GetDetailsAsync(id);
        if (task == null) return NotFound();

        await LoadSelectListsAsync();
        return View(new TaskEditViewModel
        {
            Id          = task.Id,
            Title       = task.Title,
            Description = task.Description,
            Priority    = task.Priority,
            Status      = task.Status,
            Deadline    = task.Deadline,
            AssignedToId = task.AssignedToId,
            DependsOnId = task.DependsOnId,
        });
    }

    // POST /Task/Edit/5
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, TaskEditViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (vm.StartDate.HasValue && vm.Deadline.HasValue && vm.StartDate.Value > vm.Deadline.Value)
            ModelState.AddModelError(nameof(vm.Deadline), "Deadline must not be before start date.");

        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return View(vm);
        }

        var actorId = _userManager.GetUserId(User)!;
        await _tasks.UpdateAsync(vm, actorId);
        TempData["Success"] = "Task updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST /Task/Delete/5
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _tasks.DeleteAsync(id);
        TempData["Success"] = "Task deleted.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Task/UpdateStatus
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin,TeamLead,Developer,Member")]
    public async Task<IActionResult> UpdateStatus(int taskId, AgileTaskManager.Models.Enums.TaskStatus newStatus)
    {
        var actorId = _userManager.GetUserId(User)!;
        try
        {
            await _tasks.UpdateStatusAsync(taskId, newStatus, actorId);
            TempData["Success"] = "Status updated.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id = taskId });
    }

    // POST /Task/Assign
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Assign(int taskId, string userId)
    {
        var actorId = _userManager.GetUserId(User)!;
        await _tasks.AssignAsync(taskId, userId, actorId);
        TempData["Success"] = "Task assigned.";
        return RedirectToAction(nameof(Details), new { id = taskId });
    }

    // GET /Task/ExportExcel
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExportExcel()
    {
        var tasks = await _tasks.GetAllAsync();
        var bytes = _export.ExportTasksToExcel(tasks);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Tasks_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    // POST /Task/AddComment
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin,TeamLead,Developer,Viewer,Member,Client")]
    public async Task<IActionResult> AddComment(AddCommentViewModel vm)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(Details), new { id = vm.TaskId });
        var userId = _userManager.GetUserId(User)!;
        await _comments.AddAsync(vm, userId);
        return RedirectToAction(nameof(Details), new { id = vm.TaskId });
    }

    // POST /Task/UploadAttachment
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin,TeamLead,Developer,Viewer,Member,Client")]
    public async Task<IActionResult> UploadAttachment(int taskId, IFormFile? file)
    {
        var taskExists = await _db.Tasks.AnyAsync(t => t.Id == taskId);
        if (!taskExists)
            return NotFound();

        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select a file to upload.";
            return RedirectToAction(nameof(Details), new { id = taskId });
        }

        const long maxSizeBytes = 10 * 1024 * 1024; // 10 MB
        if (file.Length > maxSizeBytes)
        {
            TempData["Error"] = "File is too large. Maximum allowed size is 10 MB.";
            return RedirectToAction(nameof(Details), new { id = taskId });
        }

        var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "tasks", taskId.ToString());
        Directory.CreateDirectory(uploadsRoot);

        var safeOriginalName = Path.GetFileName(file.FileName);
        var extension = Path.GetExtension(safeOriginalName);
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var storedPath = Path.Combine(uploadsRoot, storedName);

        await using (var stream = new FileStream(storedPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var attachments = await _db.Attachments
            .Where(a => a.TaskId == taskId)
            .Select(a => new AttachmentViewModel
            {
                Id = a.Id,
                FileName = a.FileName,
                FilePath = a.FilePath,
                FileSizeBytes = a.FileSizeBytes,
                UploadedAt = a.UploadedAt
            })
            .ToListAsync();

        _db.Attachments.Add(new Attachment
        {
            TaskId = taskId,
            FileName = safeOriginalName,
            FilePath = $"/uploads/tasks/{taskId}/{storedName}",
            FileSizeBytes = file.Length,
            UploadedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = "Attachment uploaded successfully.";
        return RedirectToAction(nameof(Details), new { id = taskId });
    }

    // GET /Task/DownloadAttachment/5
    [Authorize(Roles = "Admin,TeamLead,Developer,Viewer,Member,Client")]
    public async Task<IActionResult> DownloadAttachment(int id)
    {
        var attachment = await _db.Attachments.FindAsync(id);
        if (attachment == null)
            return NotFound();

        var relativePath = attachment.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_env.WebRootPath, relativePath);

        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        return PhysicalFile(fullPath, "application/octet-stream", attachment.FileName);
    }

    // POST /Task/DeleteComment
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment(int commentId, int taskId)
    {
        var userId  = _userManager.GetUserId(User)!;
        var isAdmin = User.IsInRole(RoleConstants.Admin);
        await _comments.DeleteAsync(commentId, userId, isAdmin);
        return RedirectToAction(nameof(Details), new { id = taskId });
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private async Task LoadSelectListsAsync()
    {
        var users = await _users.GetAllAsync();
        ViewBag.Members = new SelectList(
            users.Where(u => u.Role == "Admin" || u.Role == "Developer" || u.Role == "Developer" || u.Role == "TeamLead"),
            "Id", "FullName");
        ViewBag.AllTasks = await _tasks.GetAllAsync();
    }
}
