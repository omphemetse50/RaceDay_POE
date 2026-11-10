using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class EventsController : Controller
{
    private readonly ApiClient _api;

    public EventsController(ApiClient api)
    {
        _api = api;
    }

    /// <summary>Public: lists all upcoming events, with optional filtering. Anyone can view.</summary>
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? location, DateTime? afterDate)
    {
        var result = await _api.GetEventsAsync(location, afterDate);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Error;
            return View(new EventListFilterViewModel());
        }

        return View(new EventListFilterViewModel
        {
            Events = result.Data!,
            Location = location,
            AfterDate = afterDate
        });
    }

    /// <summary>Public: full event details including categories.</summary>
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var result = await _api.GetEventAsync(id);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [Authorize(Roles = "Organiser")]
    public IActionResult Create() => View(new EventFormViewModel());

    [Authorize(Roles = "Organiser")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.CreateEventAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not create the event.");
            return View(model);
        }

        if (model.BannerImage is not null && model.BannerImage.Length > 0)
        {
            await _api.UploadEventBannerAsync(result.Data!.EventId, model.BannerImage);
        }

        TempData["SuccessMessage"] = "Event created.";
        return RedirectToAction(nameof(Details), new { id = result.Data!.EventId });
    }

    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Edit(int id)
    {
        var result = await _api.GetEventAsync(id);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var ev = result.Data!;
        if (!IsOwner(ev)) return Forbid();

        return View(new EventFormViewModel
        {
            EventId = ev.EventId,
            Name = ev.Name,
            Description = ev.Description,
            EventDate = ev.EventDate,
            Location = ev.Location,
            DistanceKm = ev.DistanceKm,
            EventType = ev.EventType,
            ExistingBannerUrl = ev.BannerImageUrl
        });
    }

    [Authorize(Roles = "Organiser")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EventFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.UpdateEventAsync(id, model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not update the event.");
            return View(model);
        }

        if (model.BannerImage is not null && model.BannerImage.Length > 0)
        {
            await _api.UploadEventBannerAsync(id, model.BannerImage);
        }

        TempData["SuccessMessage"] = "Event updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Organiser")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _api.DeleteEventAsync(id);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Error;
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["SuccessMessage"] = "Event deleted.";
        return RedirectToAction("Index", "Dashboard");
    }

    /// <summary>Organiser: view all participants enrolled across every category of this event.</summary>
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Enrolments(int id)
    {
        var eventResult = await _api.GetEventAsync(id);
        if (!eventResult.Success)
        {
            TempData["ErrorMessage"] = eventResult.Error;
            return RedirectToAction("Index", "Dashboard");
        }

        var enrolmentsResult = await _api.GetEventEnrolmentsAsync(id);
        ViewBag.Event = eventResult.Data;

        if (!enrolmentsResult.Success)
        {
            TempData["ErrorMessage"] = enrolmentsResult.Error;
            return View(new List<ApiEnrolment>());
        }

        return View(enrolmentsResult.Data);
    }

    private bool IsOwner(ApiEvent ev)
    {
        var myUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return ev.OrganiserId == myUserId;
    }
}
