using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[Authorize(Roles = "Organiser")]
public class CategoriesController : Controller
{
    private readonly ApiClient _api;

    public CategoriesController(ApiClient api)
    {
        _api = api;
    }

    /// <summary>Category management page for a specific event -- list + add form.</summary>
    public async Task<IActionResult> Index(int eventId)
    {
        var eventResult = await _api.GetEventAsync(eventId);
        if (!eventResult.Success)
        {
            TempData["ErrorMessage"] = eventResult.Error;
            return RedirectToAction("Index", "Dashboard");
        }

        var categoriesResult = await _api.GetCategoriesAsync(eventId);

        var vm = new CategoriesPageViewModel
        {
            Event = eventResult.Data!,
            Categories = categoriesResult.Success ? categoriesResult.Data! : new List<ApiCategory>(),
            NewCategory = new CategoryFormViewModel { EventId = eventId, EventName = eventResult.Data!.Name }
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "NewCategory")] CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please fill in all required category fields.";
            return RedirectToAction(nameof(Index), new { eventId = model.EventId });
        }

        var result = await _api.CreateCategoryAsync(model.EventId, model);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "Category added." : result.Error;

        return RedirectToAction(nameof(Index), new { eventId = model.EventId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryFormViewModel model)
    {
        var result = await _api.UpdateCategoryAsync(model.CategoryId!.Value, model);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "Category updated." : result.Error;

        return RedirectToAction(nameof(Index), new { eventId = model.EventId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int categoryId, int eventId)
    {
        var result = await _api.DeleteCategoryAsync(categoryId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "Category deleted." : result.Error;

        return RedirectToAction(nameof(Index), new { eventId });
    }
}
