using AgileTaskManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AgileTaskManager.Models.Entities;

namespace AgileTaskManager.Controllers;

[Authorize]
public class MeetingController : Controller
{
    private readonly IMeetingService _meetings;
    private readonly UserManager<ApplicationUser> _userManager;

    public MeetingController(IMeetingService meetings, UserManager<ApplicationUser> userManager)
    {
        _meetings = meetings;
        _userManager = userManager;
    }

    // GET: /Meeting/Dashboard
    public async Task<IActionResult> Dashboard()
    {
        var userId = _userManager.GetUserId(User)!;
        var vm = await _meetings.GetDashboardAsync(userId);
        return View(vm);
    }

    // GET: /Meeting/Detail/{meetingId}
    public async Task<IActionResult> Detail(int meetingId)
    {
        try
        {
            var vm = await _meetings.GetMeetingAsync(meetingId);
            return View(vm);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    // GET: /Meeting/Standup
    public IActionResult Standup()
    {
        return View();
    }

    // GET: /Meeting/Retrospective/{meetingId}
    public IActionResult Retrospective(int meetingId)
    {
        ViewBag.MeetingId = meetingId;
        return View();
    }
}
