using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

/// <summary>Organiser-only landing page: summary of their events, total enrolments, upcoming dates.</summary>
[Authorize(Roles = "Organiser")]
public class DashboardController : Controller
{
    private readonly ApiClient _api;

    public DashboardController(ApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var eventsResult = await _api.GetEventsAsync();
        if (!eventsResult.Success)
        {
            TempData["ErrorMessage"] = eventsResult.Error;
            return View(new OrganiserDashboardViewModel());
        }

        var myUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var myEvents = eventsResult.Data!.Where(e => e.OrganiserId == myUserId).ToList();

        var dashboardEvents = new List<DashboardEventDto>();
        var totalEnrolments = 0;

        foreach (var ev in myEvents)
        {
            var enrolmentsResult = await _api.GetEventEnrolmentsAsync(ev.EventId);
            var count = enrolmentsResult.Success ? enrolmentsResult.Data!.Count : 0;
            totalEnrolments += count;

            dashboardEvents.Add(new DashboardEventDto { Event = ev, EnrolmentCount = count });
        }

        var vm = new OrganiserDashboardViewModel
        {
            MyEvents = dashboardEvents,
            TotalEnrolments = totalEnrolments,
            NextEventDate = myEvents.Where(e => e.EventDate >= DateTime.Today).OrderBy(e => e.EventDate).FirstOrDefault()?.EventDate
        };

        return View(vm);
    }
}