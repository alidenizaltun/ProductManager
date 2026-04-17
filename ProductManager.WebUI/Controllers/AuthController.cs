using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProductManager.Domain.Entities;
using ProductManager.WebUI.Models.Auth;

namespace ProductManager.WebUI.Controllers;

public sealed class AuthController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    private const string SuperAdminEmail = "pm@gmail.com";
    private const string SuperAdminRole = "SuperAdmin";

    public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [AllowAnonymous]
    [HttpGet("/login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [AllowAnonymous]
    [HttpPost("/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var normalizedInput = model.UserNameOrEmail?.Trim();
        if (!string.Equals(normalizedInput, SuperAdminEmail, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "Bu panelde sadece super admin girisi yetkilidir.");
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(SuperAdminEmail);
        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Super admin hesabi aktif degil.");
            return View(model);
        }

        if (!await _userManager.IsInRoleAsync(user, SuperAdminRole))
        {
            ModelState.AddModelError(string.Empty, "Super admin rol yetkisi bulunamadi.");
            return View(model);
        }

        var signInResult = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Cok fazla hatali deneme nedeniyle hesap gecici olarak kilitlendi.");
            return View(model);
        }

        if (!signInResult.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Gecersiz email veya sifre.");
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return LocalRedirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost("/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
}
