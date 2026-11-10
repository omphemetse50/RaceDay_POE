using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Web.Controllers;

public class HomeController : Controller
{
    /// <summary>Public landing: sends everyone to the role-appropriate home page.</summary>
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Organiser"))
        {
            return RedirectToAction("Index", "Dashboard");
        }

        // Participants and anonymous visitors both land on the public event browse page.
        return RedirectToAction("Index", "Events");
    }

    public IActionResult Error() => View();
}
