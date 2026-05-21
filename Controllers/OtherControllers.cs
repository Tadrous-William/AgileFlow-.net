using AgileTaskManager.Models.Entities;
using AgileTaskManager.Models.ViewModels;
using AgileTaskManager.Services.Interfaces;
using AgileTaskManager.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgileTaskManager.Controllers;

// ── DashboardController ───────────────────────────────────────────────────────
[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboard;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(IDashboardService dashboard, UserManager<ApplicationUser> userManager)
    { _dashboard = dashboard; _userManager = userManager; }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;

        if (User.IsInRole(RoleConstants.Admin))
        {
            var adminVm = await _dashboard.GetAdminDashboardEnhancedAsync();
            return View("AdminDashboard", adminVm);
        }
        else if (User.IsInRole(RoleConstants.TeamLead))
        {
            var teamLeadVm = await _dashboard.GetTeamLeadDashboardAsync(userId);
            return View("TeamLeadDashboard", teamLeadVm);
        }
        else if (User.IsInRole(RoleConstants.Developer))
        {
            var developerVm = await _dashboard.GetDeveloperDashboardAsync(userId);
            return View("DeveloperDashboard", developerVm);
        }
        else // Client
        {
            var clientVm = await _dashboard.GetClientDashboardEnhancedAsync(userId);
            return View("ClientDashboard", clientVm);
        }
    }
}

// ── AccountController ─────────────────────────────────────────────────────────
public class AccountController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signIn;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signIn, IConfiguration configuration)
    { _userManager = userManager; _signIn = signIn; _configuration = configuration; }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _signIn.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
            return LocalRedirect(returnUrl ?? "/");

        ModelState.AddModelError("", "Invalid login attempt.");
        return View(vm);
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = new ApplicationUser
        {
            FullName = vm.FullName,
            UserName = vm.Email,
            Email    = vm.Email,
            Role     = vm.Role
        };
        var result = await _userManager.CreateAsync(user, vm.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, vm.Role);
            await _signIn.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Dashboard");
        }

        foreach (var e in result.Errors)
            ModelState.AddModelError("", e.Description);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction("Login");
    }

    public IActionResult AccessDenied() => View();
}

// ── UserController ────────────────────────────────────────────────────────────
[Authorize(Roles = "Admin")]
public class UserController : Controller
{
    private readonly IUserService _users;

    public UserController(IUserService users) => _users = users;

    public async Task<IActionResult> Index()
        => View(await _users.GetAllAsync());

    public async Task<IActionResult> Details(string id)
    {
        var user = await _users.GetByIdAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRole(AssignRoleViewModel vm)
    {
        try
        {
            await _users.AssignRoleAsync(vm.UserId, vm.NewRole);
            TempData["Success"] = "Developer promoted to Admin successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id)
    {
        await _users.ToggleActiveAsync(id);
        return RedirectToAction(nameof(Index));
    }
}

// ── NotificationController ────────────────────────────────────────────────────
[Authorize]
public class NotificationController : Controller
{
    private readonly INotificationService _notifications;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationController(INotificationService notifications, UserManager<ApplicationUser> userManager)
    { _notifications = notifications; _userManager = userManager; }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        return View(await _notifications.GetUserNotificationsAsync(userId));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        await _notifications.MarkAsReadAsync(id, userId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = _userManager.GetUserId(User)!;
        await _notifications.MarkAllAsReadAsync(userId);
        return RedirectToAction(nameof(Index));
    }
}

// ── FeedbackController ────────────────────────────────────────────────────────
[Authorize(Roles = "Client")]
public class FeedbackController : Controller
{
    private readonly IFeedbackService _feedback;
    private readonly UserManager<ApplicationUser> _userManager;

    public FeedbackController(IFeedbackService feedback, UserManager<ApplicationUser> userManager)
    { _feedback = feedback; _userManager = userManager; }

    [HttpGet]
    public IActionResult Add(int taskId) => View(new AddFeedbackViewModel { TaskId = taskId });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(AddFeedbackViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var clientId = _userManager.GetUserId(User)!;
        await _feedback.AddAsync(vm, clientId);
        TempData["Success"] = "Feedback submitted. Thank you!";
        return RedirectToAction("Details", "Task", new { id = vm.TaskId });
    }
}
