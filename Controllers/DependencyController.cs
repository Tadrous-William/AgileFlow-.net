using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileTaskManager.Controllers;

[Authorize]
public class DependencyController : Controller
{
    // GET: /Dependency/Graph/{id}
    public IActionResult Graph(int id)
    {
        if (id <= 0)
            return RedirectToAction("Index", "Task");

        ViewBag.TaskId = id;
        return View();
    }

    // GET: /Dependency/Manage/{id}
    public IActionResult Manage(int id)
    {
        if (id <= 0)
            return RedirectToAction("Index", "Task");

        ViewBag.TaskId = id;
        return View();
    }
}
