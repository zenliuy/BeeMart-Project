using System.Security.Claims;
using BeeMart.WebClient.Models;
using BeeMart.WebClient.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace BeeMart.WebClient.Controllers;

public class AccountController : Controller
{
    private readonly ApiClient _api;

    public AccountController(ApiClient api)
    {
        _api = api;
    }

    // ---------- LOGIN ----------
    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrEmpty(model.Password))
        {
            model.Error = "Username dan Password wajib diisi.";
            return View(model);
        }

        var role = model.Role?.Trim() ?? string.Empty;
        var (result, error) = await _api.LoginAsync(model.Username.Trim(), model.Password!, role);

        if (result == null)
        {
            model.Error = error;
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, result.User.Username),
            new(ClaimTypes.Role, result.User.Role),
            new("idUser", result.User.IdUser),
            new("fullName", result.User.FullName),
            new("token", result.Token)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var props = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(120)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            props);

        return RedirectToAction("Index", "Home");
    }

    // ---------- REGISTER ----------
    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.FullName)
            || string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.RePassword))
        {
            model.Error = "Username, Full Name, Password dan Re-Password wajib diisi.";
            return View(model);
        }

        if (model.Password != model.RePassword)
        {
            model.Error = "Re-Password harus sama dengan Password.";
            return View(model);
        }

        var role = model.Role?.Trim() ?? string.Empty;
        var error = await _api.RegisterAsync(model.Username.Trim(), model.FullName.Trim(),
            model.Password!, model.RePassword!, role);

        if (error != null)
        {
            model.Error = error;
            return View(model);
        }

        TempData["Success"] = "Registrasi berhasil. Silakan login.";
        return RedirectToAction("Login");
    }

    // ---------- LOGOUT ----------
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}