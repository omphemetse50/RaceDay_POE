using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[Authorize]
public class ResultsController : Controller
{
    private readonly ApiClient _api;

    public ResultsController(ApiClient api)
    {
        _api = api;
    }

    /// <summary>Participant: their personal race history across every completed event.</summary>
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> MyResults()
    {
        var result = await _api.GetMyResultsAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Error;
            return View(new MyResultsViewModel());
        }

        return View(new MyResultsViewModel { Results = result.Data! });
    }

    /// <summary>Organiser: the finish-line result capture form for one enrolled participant.</summary>
    [Authorize(Roles = "Organiser")]
    public IActionResult Capture(int enrolmentId, int eventId)
    {
        ViewBag.EventId = eventId;
        return View(new ResultFormViewModel { EnrolmentId = enrolmentId });
    }

    [Authorize(Roles = "Organiser")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Capture(ResultFormViewModel model, int eventId)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.EventId = eventId;
            return View(model);
        }

        var result = await _api.CaptureResultAsync(model.EnrolmentId, model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not capture the result.");
            ViewBag.EventId = eventId;
            return View(model);
        }

        TempData["SuccessMessage"] = "Result captured.";
        return RedirectToAction("Enrolments", "Events", new { id = eventId });
    }
}
