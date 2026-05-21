using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileTaskManager.Controllers;

[Authorize]
public class AIAssistantController : Controller
{
    // GET: /AIAssistant
    public IActionResult Index()
    {
        return View();
    }
}
