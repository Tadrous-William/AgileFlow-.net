using AgileTaskManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AgileTaskManager.Models.Entities;

namespace AgileTaskManager.Controllers;

[Authorize]
public class TimeTrackingController : Controller
{
    private readonly ITimeTrackingService _timeTracking;
    private readonly UserManager<ApplicationUser> _userManager;

    public TimeTrackingController(ITimeTrackingService timeTracking, UserManager<ApplicationUser> userManager)
    {
        _timeTracking = timeTracking;
        _userManager = userManager;
    }

    // GET: /TimeTracking/Dashboard
    public async Task<IActionResult> Dashboard()
    {
        var userId = _userManager.GetUserId(User)!;
        var vm = await _timeTracking.GetDashboardAsync(userId);
        return View(vm);
    }

    // GET: /TimeTracking/Timer
    public IActionResult Timer()
    {
        return View();
    }

    // GET: /TimeTracking/Reports
    public IActionResult Reports()
    {
        return View();
    }
}
