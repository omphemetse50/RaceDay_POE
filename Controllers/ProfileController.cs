using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ApiClient _api;

    public ProfileController(ApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _api.GetMyProfileAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Error;
            return RedirectToAction("Index", "Home");
        }

        var p = result.Data!;
        return View(new ProfileViewModel
        {
            FullName = p.FullName,
            Email = p.Email,
            Role = p.Role,
            ProfilePictureUrl = p.ProfilePictureUrl,
            CreatedAt = p.CreatedAt
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var updateResult = await _api.UpdateMyProfileAsync(model.FullName, model.Email);
        if (!updateResult.Success)
        {
            ModelState.AddModelError(string.Empty, updateResult.Error ?? "Could not update your profile.");
            return View(model);
        }

        if (model.ProfilePicture is not null && model.ProfilePicture.Length > 0)
        {
            var uploadResult = await _api.UploadProfilePictureAsync(model.ProfilePicture);
            if (!uploadResult.Success)
            {
                TempData["ErrorMessage"] = uploadResult.Error;
            }
        }

        TempData["SuccessMessage"] = "Profile updated.";
        return RedirectToAction(nameof(Index));
    }
}
