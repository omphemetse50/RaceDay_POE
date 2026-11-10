using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[Authorize(Roles = "Participant")]
public class EnrolmentsController : Controller
{
    private readonly ApiClient _api;

    public EnrolmentsController(ApiClient api)
    {
        _api = api;
    }

    /// <summary>The logged-in Participant's personal view of all enrolments and their status.</summary>
    public async Task<IActionResult> Index()
    {
        var result = await _api.GetMyEnrolmentsAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Error;
            return View(new List<Models.ApiEnrolment>());
        }

        return View(result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int categoryId, int eventId)
    {
        var result = await _api.EnrolAsync(categoryId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "You're enrolled! Good luck." : result.Error;

        return RedirectToAction("Details", "Events", new { id = eventId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int enrolmentId)
    {
        var result = await _api.CancelEnrolmentAsync(enrolmentId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "Enrolment cancelled." : result.Error;

        return RedirectToAction(nameof(Index));
    }
}
