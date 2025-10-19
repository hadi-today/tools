using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Tools.Models;

namespace Tools.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public SettingsController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        var model = new SettingsViewModel
        {
            HourlyRate = user.HourlyRate,
            OpenAIApiKey = user.OpenAIApiKey,
            GeminiApiKey = user.GeminiApiKey
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        user.HourlyRate = model.HourlyRate;
        user.OpenAIApiKey = string.IsNullOrWhiteSpace(model.OpenAIApiKey) ? null : model.OpenAIApiKey.Trim();
        user.GeminiApiKey = string.IsNullOrWhiteSpace(model.GeminiApiKey) ? null : model.GeminiApiKey.Trim();

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        TempData["StatusMessage"] = "Settings updated successfully.";

        return RedirectToAction(nameof(Index));
    }
}
